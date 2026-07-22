using Koop.Data.Context;
using Koop.Entity.DTOs.Accounting;
using Koop.Entity.Entities;
using Koop.Service.Services.AccountingServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace WebApi.Controllers.Accounting
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountingController : ControllerBase
    {
        private const decimal GrossMultiplier = 1.104m;
        private readonly AppDbContext _context;
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(AppDbContext context, ILogger<AccountingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("vehicles")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetVehicles()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.AppUser)
                .OrderBy(v => v.LicensePlate)
                .Select(v => new AccountingVehicleDto
                {
                    Id = v.Id,
                    LicensePlate = v.LicensePlate,
                    DriverName = v.AppUser != null ? v.AppUser.FullName : v.DriverName,
                    AppUserId = v.AppUserId,
                    UserFullName = v.AppUser != null ? v.AppUser.FullName : "Atanmadı"
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetUsers()
        {
            var userRoleId = await _context.Roles
                .Where(r => r.NormalizedName == "USER")
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync();

            if (userRoleId == null)
            {
                return Ok(new List<AccountingUserDto>());
            }

            var users = await _context.Users
                .Include(u => u.Vehicles.OrderBy(v => v.LicensePlate))
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == userRoleId.Value))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var transactions = await _context.AccountingTransactions
                .Where(r => userIds.Contains(r.UserId))
                .ToListAsync();

            var result = users.Select(user =>
            {
                var userTransactions = transactions
                    .Where(r => r.UserId == user.Id)
                    .ToList();

                return new AccountingUserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Vehicles = user.Vehicles
                        .Select(v => new AccountingVehicleDto
                        {
                            Id = v.Id,
                            LicensePlate = v.LicensePlate,
                            DriverName = v.DriverName,
                            AppUserId = v.AppUserId,
                            UserFullName = user.FullName
                        })
                        .ToList(),
                    IncomeTotal = userTransactions.Where(r => r.TransactionType == AccountingTransactionType.Credit).Sum(r => r.Amount),
                    ExpenseTotal = userTransactions.Where(r => r.TransactionType == AccountingTransactionType.Debit).Sum(r => r.Amount),
                    PaymentTotal = 0,
                    Balance = userTransactions.Sum(GetTransactionEffect),
                    RecordCount = userTransactions.Count
                };
            }).ToList();

            return Ok(result);
        }

        [HttpGet("users/{userId:guid}/ledger")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetUserLedger(Guid userId, [FromQuery] int? periodMonth, [FromQuery] int? periodYear, [FromQuery] string? sort)
        {
            var user = await GetNormalUserAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            if (!TryGetPeriod(periodMonth, periodYear, out var month, out var year, out var periodError))
            {
                return BadRequest(new { message = periodError });
            }

            var ledger = await BuildLedgerAsync(user, month, year, sort);
            return Ok(ledger);
        }

        [HttpPost("users/{userId:guid}/transactions")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> CreateTransaction(Guid userId, [FromBody] CreateAccountingTransactionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var validationError = AccountingLedgerCalculator.ValidateInput(dto.TransactionDate, dto.Description, dto.Type, dto.Amount);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            var user = await GetNormalUserAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var transaction = new AccountingTransaction
            {
                UserId = user.Id,
                TransactionDate = dto.TransactionDate.Date,
                Description = dto.Description.Trim(),
                TransactionType = dto.Type,
                Amount = Math.Round(dto.Amount, 2),
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.AccountingTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(ToTransactionDto(transaction, GetTransactionEffect(transaction)));
        }

        [HttpPut("transactions/{id:long}")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> UpdateTransaction(long id, [FromBody] UpdateAccountingTransactionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var validationError = AccountingLedgerCalculator.ValidateInput(dto.TransactionDate, dto.Description, dto.Type, dto.Amount);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            var transaction = await _context.AccountingTransactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound("Cari hareket bulunamadi.");
            }

            transaction.TransactionDate = dto.TransactionDate.Date;
            transaction.Description = dto.Description.Trim();
            transaction.TransactionType = dto.Type;
            transaction.Amount = Math.Round(dto.Amount, 2);
            transaction.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("transactions/{id:long}")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> DeleteTransaction(long id)
        {
            var transaction = await _context.AccountingTransactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound("Cari hareket bulunamadi.");
            }

            _context.AccountingTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("my-ledger")]
        public async Task<IActionResult> GetMyLedger([FromQuery] int? periodMonth, [FromQuery] int? periodYear, [FromQuery] string? sort)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            if (!TryGetPeriod(periodMonth, periodYear, out var month, out var year, out var periodError))
            {
                return BadRequest(new { message = periodError });
            }

            var ledger = await BuildLedgerAsync(user, month, year, sort);
            return Ok(ledger);
        }

        [HttpGet("users/{userId:guid}/records")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetUserRecords(Guid userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            var records = await ApplyRecordFilters(_context.AccountingRecords.Where(r => r.Vehicle.AppUserId == userId), startDate, endDate, category)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            return Ok(records.Select(ToDto).ToList());
        }

        [HttpGet("users/{userId:guid}/summary")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetUserSummary(Guid userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            var records = await ApplyRecordFilters(_context.AccountingRecords.Where(r => r.Vehicle.AppUserId == userId), startDate, endDate, category)
                .ToListAsync();

            return Ok(ToUserSummary(user, records));
        }

        [HttpGet("users/{userId:guid}/monthly-summary")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetUserMonthlySummary(Guid userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            var records = await ApplyRecordFilters(_context.AccountingRecords.Where(r => r.Vehicle.AppUserId == userId), startDate, endDate, category)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.Id)
                .ToListAsync();

            return Ok(ToMonthlySummaries(records));
        }

        [HttpGet("monthly-summaries")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetMonthlySummaries(
            [FromQuery] Guid? userId,
            [FromQuery] long? vehicleId,
            [FromQuery] int? periodMonth,
            [FromQuery] int? periodYear)
        {
            var query = ApplyMonthlySummaryFilters(_context.AccountingMonthlySummaries.AsQueryable(), userId, vehicleId, periodMonth, periodYear);

            var summaries = await query
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .OrderByDescending(s => s.PeriodYear)
                .ThenByDescending(s => s.PeriodMonth)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            return Ok(summaries.Select(ToMonthlySummaryDto).ToList());
        }

        [HttpPost("monthly-summaries")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> CreateMonthlySummary([FromBody] CreateAccountingMonthlySummaryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.AppUser)
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

            if (vehicle == null)
            {
                return NotFound("Arac bulunamadi.");
            }

            if (vehicle.AppUserId == null)
            {
                return BadRequest(new { message = "Secilen araca atanmis kullanici bulunmuyor." });
            }

            if (dto.UserId.HasValue && dto.UserId.Value != vehicle.AppUserId.Value)
            {
                return BadRequest(new { message = "Secilen arac bu kullaniciya ait degil." });
            }

            if (string.IsNullOrWhiteSpace(vehicle.LicensePlate))
            {
                return BadRequest(new { message = "Secilen aracin plaka bilgisi bos." });
            }

            var duplicateExists = await _context.AccountingMonthlySummaries.AnyAsync(s =>
                s.UserId == vehicle.AppUserId &&
                s.VehicleId == vehicle.Id &&
                s.PeriodYear == dto.PeriodYear &&
                s.PeriodMonth == dto.PeriodMonth);

            if (duplicateExists)
            {
                return BadRequest(new { message = "Bu kullanici ve plaka icin secilen donemde cari ozet zaten var." });
            }

            var summary = new AccountingMonthlySummary
            {
                UserId = vehicle.AppUserId,
                VehicleId = vehicle.Id,
                PlateNumber = vehicle.LicensePlate,
                PeriodMonth = dto.PeriodMonth,
                PeriodYear = dto.PeriodYear,
                PreviousBalance = dto.PreviousBalance,
                IncomeAmount = dto.IncomeAmount,
                ExpenseAmount = dto.ExpenseAmount,
                Description = dto.Description,
                CreatedByUserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            ApplyMonthlySummaryTotal(summary);

            try
            {
                _context.AccountingMonthlySummaries.Add(summary);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex,
                    "Cari ozet kaydedilemedi. UserId: {UserId}, VehicleId: {VehicleId}, Period: {PeriodMonth}/{PeriodYear}",
                    summary.UserId,
                    summary.VehicleId,
                    summary.PeriodMonth,
                    summary.PeriodYear);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Cari ozet veritabanina kaydedilemedi. Migration uygulanmamis olabilir veya ayni donem icin cakisan kayit vardir."
                });
            }

            var createdSummary = await _context.AccountingMonthlySummaries
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .FirstAsync(s => s.Id == summary.Id);

            return Ok(ToMonthlySummaryDto(createdSummary));
        }

        [HttpPut("monthly-summaries/{id:long}")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> UpdateMonthlySummary(long id, [FromBody] UpdateAccountingMonthlySummaryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var summary = await _context.AccountingMonthlySummaries.FindAsync(id);
            if (summary == null)
            {
                return NotFound("Cari ozet bulunamadi.");
            }

            var vehicle = await _context.Vehicles
                .Include(v => v.AppUser)
                .FirstOrDefaultAsync(v => v.Id == dto.VehicleId);

            if (vehicle == null)
            {
                return NotFound("Arac bulunamadi.");
            }

            if (vehicle.AppUserId == null)
            {
                return BadRequest(new { message = "Secilen araca atanmis kullanici bulunmuyor." });
            }

            var duplicateExists = await _context.AccountingMonthlySummaries.AnyAsync(s =>
                s.Id != id &&
                s.UserId == vehicle.AppUserId &&
                s.VehicleId == vehicle.Id &&
                s.PeriodYear == dto.PeriodYear &&
                s.PeriodMonth == dto.PeriodMonth);

            if (duplicateExists)
            {
                return BadRequest(new { message = "Bu kullanici ve plaka icin secilen donemde cari ozet zaten var." });
            }

            summary.UserId = vehicle.AppUserId;
            summary.VehicleId = vehicle.Id;
            summary.PlateNumber = vehicle.LicensePlate;
            summary.PeriodMonth = dto.PeriodMonth;
            summary.PeriodYear = dto.PeriodYear;
            summary.PreviousBalance = dto.PreviousBalance;
            summary.IncomeAmount = dto.IncomeAmount;
            summary.ExpenseAmount = dto.ExpenseAmount;
            summary.Description = dto.Description;
            summary.UpdatedAt = DateTime.UtcNow;

            ApplyMonthlySummaryTotal(summary);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("monthly-summaries/{id:long}")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> DeleteMonthlySummary(long id)
        {
            var summary = await _context.AccountingMonthlySummaries.FindAsync(id);
            if (summary == null)
            {
                return NotFound("Cari ozet bulunamadi.");
            }

            _context.AccountingMonthlySummaries.Remove(summary);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("vehicles/{vehicleId}/records")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetVehicleRecords(long vehicleId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == vehicleId);
            if (!vehicleExists)
            {
                return NotFound("Araç bulunamadı.");
            }

            var records = await ApplyRecordFilters(_context.AccountingRecords.Where(r => r.VehicleId == vehicleId), startDate, endDate, category)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            return Ok(records.Select(ToDto).ToList());
        }

        [HttpGet("vehicles/{vehicleId}/summary")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> GetVehicleSummary(long vehicleId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.AppUser)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
            {
                return NotFound("Araç bulunamadı.");
            }

            var query = ApplyRecordFilters(_context.AccountingRecords.Where(r => r.VehicleId == vehicleId), startDate, endDate, category);
            var records = await query.ToListAsync();

            return Ok(ToSummary(vehicle, records));
        }

        [HttpPost("vehicles/{vehicleId}/records")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> CreateVehicleRecord(long vehicleId, [FromBody] CreateAccountingRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound("Araç bulunamadı.");
            }

            var record = new AccountingRecord
            {
                VehicleId = vehicleId,
                Date = dto.Date,
                Type = dto.Type,
                Category = dto.Category,
                Company = dto.Company,
                WaybillNo = dto.WaybillNo,
                Description = dto.Description,
                QuantityKg = dto.QuantityKg,
                UnitPrice = dto.UnitPrice,
                CreatedByUserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            ApplyAmounts(record, dto.Amount);

            _context.AccountingRecords.Add(record);
            await _context.SaveChangesAsync();

            var createdRecord = await _context.AccountingRecords
                .Include(r => r.Vehicle)
                .FirstAsync(r => r.Id == record.Id);

            return Ok(ToDto(createdRecord));
        }

        [HttpPost("users/{userId:guid}/records")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> CreateUserRecord(Guid userId, [FromBody] CreateUserAccountingRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            var selectedVehicleId = dto.VehicleId ?? await _context.Vehicles
                .Where(v => v.AppUserId == userId)
                .OrderBy(v => v.Id)
                .Select(v => (long?)v.Id)
                .FirstOrDefaultAsync();

            if (selectedVehicleId == null)
            {
                return BadRequest(new { message = "Bu kullaniciya kayit eklemek icin once arac atamasi yapilmalidir." });
            }

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == selectedVehicleId.Value && v.AppUserId == userId);

            if (vehicle == null)
            {
                return BadRequest(new { message = "Secilen arac bu kullaniciya ait degil." });
            }

            var record = new AccountingRecord
            {
                VehicleId = vehicle.Id,
                Date = dto.Date,
                Type = dto.Type,
                Category = dto.Category,
                Company = dto.Company,
                WaybillNo = dto.WaybillNo,
                Description = dto.Description,
                QuantityKg = dto.QuantityKg,
                UnitPrice = dto.UnitPrice,
                CreatedByUserId = GetCurrentUserId(),
                CreatedAt = DateTime.UtcNow
            };

            ApplyAmounts(record, dto.Amount);

            _context.AccountingRecords.Add(record);
            await _context.SaveChangesAsync();

            var createdRecord = await _context.AccountingRecords
                .Include(r => r.Vehicle)
                .FirstAsync(r => r.Id == record.Id);

            return Ok(ToDto(createdRecord));
        }

        [HttpPut("records/{id}")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> UpdateRecord(long id, [FromBody] UpdateAccountingRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var record = await _context.AccountingRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound("Cari kayıt bulunamadı.");
            }

            record.Date = dto.Date;
            record.Type = dto.Type;
            record.Category = dto.Category;
            record.Company = dto.Company;
            record.WaybillNo = dto.WaybillNo;
            record.Description = dto.Description;
            record.QuantityKg = dto.QuantityKg;
            record.UnitPrice = dto.UnitPrice;

            ApplyAmounts(record, dto.Amount);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("records/{id}")]
        [Authorize(Roles = "Admin,Muhasebeci")]
        public async Task<IActionResult> DeleteRecord(long id)
        {
            var record = await _context.AccountingRecords.FindAsync(id);
            if (record == null)
            {
                return NotFound("Cari kayıt bulunamadı.");
            }

            _context.AccountingRecords.Remove(record);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("my-records")]
        public async Task<IActionResult> GetMyRecords([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var records = await ApplyRecordFilters(
                    _context.AccountingRecords.Where(r => r.Vehicle.AppUserId == userId),
                    startDate,
                    endDate,
                    category)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.Date)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            return Ok(records.Select(ToDto).ToList());
        }

        [HttpGet("my-summary")]
        public async Task<IActionResult> GetMySummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var vehicles = await _context.Vehicles
                .Include(v => v.AppUser)
                .Where(v => v.AppUserId == userId)
                .ToListAsync();

            var vehicleIds = vehicles.Select(v => v.Id).ToList();
            var records = await ApplyRecordFilters(
                    _context.AccountingRecords.Where(r => vehicleIds.Contains(r.VehicleId)),
                    startDate,
                    endDate,
                    category)
                .ToListAsync();

            var summaries = vehicles
                .Select(v => ToSummary(v, records.Where(r => r.VehicleId == v.Id).ToList()))
                .ToList();

            return Ok(summaries);
        }

        [HttpGet("my-user-summary")]
        public async Task<IActionResult> GetMyUserSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null)
            {
                return NotFound("Kullanici bulunamadi.");
            }

            var records = await ApplyRecordFilters(
                    _context.AccountingRecords.Where(r => r.Vehicle.AppUserId == userId),
                    startDate,
                    endDate,
                    category)
                .ToListAsync();

            return Ok(ToUserSummary(user, records));
        }

        [HttpGet("my-monthly-summary")]
        public async Task<IActionResult> GetMyMonthlySummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? category)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var records = await ApplyRecordFilters(
                    _context.AccountingRecords.Where(r => r.Vehicle.AppUserId == userId),
                    startDate,
                    endDate,
                    category)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.Id)
                .ToListAsync();

            return Ok(ToMonthlySummaries(records));
        }

        [HttpGet("my-monthly-summaries")]
        public async Task<IActionResult> GetMyMonthlySummaries([FromQuery] int? periodMonth, [FromQuery] int? periodYear)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var summaries = await ApplyMonthlySummaryFilters(
                    _context.AccountingMonthlySummaries.Where(s => s.UserId == userId),
                    userId,
                    null,
                    periodMonth,
                    periodYear)
                .Include(s => s.User)
                .Include(s => s.Vehicle)
                .OrderByDescending(s => s.PeriodYear)
                .ThenByDescending(s => s.PeriodMonth)
                .ThenByDescending(s => s.Id)
                .ToListAsync();

            return Ok(summaries.Select(ToMonthlySummaryDto).ToList());
        }

        private async Task<AppUser?> GetNormalUserAsync(Guid userId)
        {
            var userRoleId = await _context.Roles
                .Where(r => r.NormalizedName == "USER")
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync();

            if (userRoleId == null)
            {
                return null;
            }

            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == userRoleId.Value));
        }

        private async Task<AccountingLedgerDto> BuildLedgerAsync(AppUser user, int periodMonth, int periodYear, string? sort)
        {
            var periodEnd = new DateTime(periodYear, periodMonth, 1).AddMonths(1);
            var transactions = await _context.AccountingTransactions
                .Where(t => t.UserId == user.Id && t.TransactionDate < periodEnd)
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.CreatedAt)
                .ThenBy(t => t.Id)
                .ToListAsync();

            return AccountingLedgerCalculator.Calculate(user, transactions, periodMonth, periodYear, sort);
        }

        private static bool TryGetPeriod(int? periodMonth, int? periodYear, out int month, out int year, out string? error)
        {
            var today = DateTime.Today;
            month = periodMonth ?? today.Month;
            year = periodYear ?? today.Year;
            error = null;

            if (month < 1 || month > 12)
            {
                error = "Ay 1 ile 12 arasinda olmalidir.";
                return false;
            }

            if (year < 2000 || year > 2100)
            {
                error = "Yil 2000 ile 2100 arasinda olmalidir.";
                return false;
            }

            return true;
        }

        private static decimal GetTransactionEffect(AccountingTransaction transaction)
        {
            return transaction.TransactionType == AccountingTransactionType.Credit
                ? transaction.Amount
                : -transaction.Amount;
        }

        private static AccountingTransactionDto ToTransactionDto(AccountingTransaction transaction, decimal runningBalance)
        {
            return new AccountingTransactionDto
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Date = transaction.TransactionDate,
                Description = transaction.Description,
                Type = transaction.TransactionType,
                TypeName = transaction.TransactionType.ToString(),
                Amount = transaction.Amount,
                CreditAmount = transaction.TransactionType == AccountingTransactionType.Credit ? transaction.Amount : 0,
                DebitAmount = transaction.TransactionType == AccountingTransactionType.Debit ? transaction.Amount : 0,
                RunningBalance = Math.Round(runningBalance, 2),
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt
            };
        }

        private static IQueryable<AccountingRecord> ApplyRecordFilters(IQueryable<AccountingRecord> query, DateTime? startDate, DateTime? endDate, string? category)
        {
            if (startDate.HasValue)
            {
                query = query.Where(r => r.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.Date <= endDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(r => r.Category == category);
            }

            return query;
        }

        private static IQueryable<AccountingMonthlySummary> ApplyMonthlySummaryFilters(
            IQueryable<AccountingMonthlySummary> query,
            Guid? userId,
            long? vehicleId,
            int? periodMonth,
            int? periodYear)
        {
            if (userId.HasValue)
            {
                query = query.Where(s => s.UserId == userId);
            }

            if (vehicleId.HasValue)
            {
                query = query.Where(s => s.VehicleId == vehicleId);
            }

            if (periodMonth.HasValue)
            {
                query = query.Where(s => s.PeriodMonth == periodMonth);
            }

            if (periodYear.HasValue)
            {
                query = query.Where(s => s.PeriodYear == periodYear);
            }

            return query;
        }

        private static void ApplyMonthlySummaryTotal(AccountingMonthlySummary summary)
        {
            summary.TotalBalance = Math.Round(summary.PreviousBalance + summary.IncomeAmount - summary.ExpenseAmount, 2);
        }

        private static AccountingMonthlySummaryRecordDto ToMonthlySummaryDto(AccountingMonthlySummary summary)
        {
            var periodStart = new DateTime(summary.PeriodYear, summary.PeriodMonth, 1);
            return new AccountingMonthlySummaryRecordDto
            {
                Id = summary.Id,
                UserId = summary.UserId,
                UserFullName = summary.User?.FullName,
                VehicleId = summary.VehicleId,
                PlateNumber = !string.IsNullOrWhiteSpace(summary.PlateNumber)
                    ? summary.PlateNumber
                    : summary.Vehicle?.LicensePlate ?? string.Empty,
                PeriodMonth = summary.PeriodMonth,
                PeriodYear = summary.PeriodYear,
                PeriodName = periodStart.ToString("MMMM yyyy", new CultureInfo("tr-TR")),
                PreviousBalance = summary.PreviousBalance,
                IncomeAmount = summary.IncomeAmount,
                ExpenseAmount = summary.ExpenseAmount,
                TotalBalance = summary.TotalBalance,
                Description = summary.Description,
                CreatedAt = summary.CreatedAt,
                UpdatedAt = summary.UpdatedAt
            };
        }

        private static AccountingRecordDto ToDto(AccountingRecord record)
        {
            return new AccountingRecordDto
            {
                Id = record.Id,
                VehicleId = record.VehicleId,
                LicensePlate = record.Vehicle.LicensePlate,
                Date = record.Date,
                Type = record.Type,
                TypeName = record.Type.ToString(),
                Category = record.Category,
                Company = record.Company,
                WaybillNo = record.WaybillNo,
                Description = record.Description,
                QuantityKg = record.QuantityKg,
                UnitPrice = record.UnitPrice,
                NetAmount = record.NetAmount,
                GrossAmount = record.GrossAmount,
                BalanceEffect = record.BalanceEffect,
                CreatedAt = record.CreatedAt
            };
        }

        private static AccountingUserSummaryDto ToUserSummary(AppUser user, List<AccountingRecord> records)
        {
            var expenseTotal = records
                .Where(r => r.Type == AccountingRecordType.Expense)
                .Sum(r => Math.Abs(r.BalanceEffect));
            var paymentTotal = records
                .Where(r => r.Type == AccountingRecordType.Payment)
                .Sum(r => Math.Abs(r.BalanceEffect));

            return new AccountingUserSummaryDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                IncomeTotal = records.Where(r => r.BalanceEffect > 0).Sum(r => r.BalanceEffect),
                ExpenseTotal = expenseTotal,
                PaymentTotal = paymentTotal,
                OutgoingTotal = expenseTotal + paymentTotal,
                Balance = records.Sum(r => r.BalanceEffect),
                RecordCount = records.Count
            };
        }

        private static List<AccountingMonthlySummaryDto> ToMonthlySummaries(List<AccountingRecord> records)
        {
            var runningBalance = 0m;
            var culture = new CultureInfo("tr-TR");

            return records
                .GroupBy(r => new { r.Date.Year, r.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g =>
                {
                    var monthStart = new DateTime(g.Key.Year, g.Key.Month, 1);
                    var expenseTotal = g
                        .Where(r => r.Type == AccountingRecordType.Expense)
                        .Sum(r => Math.Abs(r.BalanceEffect));
                    var paymentTotal = g
                        .Where(r => r.Type == AccountingRecordType.Payment)
                        .Sum(r => Math.Abs(r.BalanceEffect));
                    var profitLoss = g.Sum(r => r.BalanceEffect);
                    runningBalance += profitLoss;

                    return new AccountingMonthlySummaryDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = monthStart.ToString("MMMM yyyy", culture),
                        MonthStart = monthStart,
                        MonthEnd = monthStart.AddMonths(1).AddTicks(-1),
                        IncomeTotal = g.Where(r => r.BalanceEffect > 0).Sum(r => r.BalanceEffect),
                        ExpenseTotal = expenseTotal,
                        PaymentTotal = paymentTotal,
                        OutgoingTotal = expenseTotal + paymentTotal,
                        ProfitLoss = profitLoss,
                        RunningBalance = runningBalance,
                        RecordCount = g.Count()
                    };
                })
                .ToList();
        }

        private static AccountingSummaryDto ToSummary(Vehicle vehicle, List<AccountingRecord> records)
        {
            return new AccountingSummaryDto
            {
                VehicleId = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                DriverName = vehicle.AppUser != null ? vehicle.AppUser.FullName : vehicle.DriverName,
                IncomeTotal = records.Where(r => r.BalanceEffect > 0).Sum(r => r.BalanceEffect),
                ExpenseTotal = records.Where(r => r.Type == AccountingRecordType.Expense).Sum(r => Math.Abs(r.BalanceEffect)),
                PaymentTotal = records.Where(r => r.Type == AccountingRecordType.Payment).Sum(r => Math.Abs(r.BalanceEffect)),
                Balance = records.Sum(r => r.BalanceEffect),
                RecordCount = records.Count
            };
        }

        private static void ApplyAmounts(AccountingRecord record, decimal? manualAmount)
        {
            var amount = record.QuantityKg.HasValue && record.UnitPrice.HasValue
                ? record.QuantityKg.Value * record.UnitPrice.Value
                : manualAmount ?? 0;

            record.NetAmount = Math.Round(amount, 2);
            record.GrossAmount = Math.Round(record.NetAmount * GrossMultiplier, 2);

            record.BalanceEffect = record.Type switch
            {
                AccountingRecordType.Income => record.GrossAmount,
                AccountingRecordType.OpeningBalance => record.NetAmount,
                AccountingRecordType.Adjustment => record.NetAmount,
                AccountingRecordType.Expense => -Math.Abs(record.NetAmount),
                AccountingRecordType.Payment => -Math.Abs(record.NetAmount),
                _ => record.NetAmount
            };
        }

        private Guid? GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }
}
