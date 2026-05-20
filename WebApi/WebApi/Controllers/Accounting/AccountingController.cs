using Koop.Data.Context;
using Koop.Entity.DTOs.Accounting;
using Koop.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AccountingController(AppDbContext context)
        {
            _context = context;
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

            return CreatedAtAction(nameof(GetVehicleRecords), new { vehicleId }, ToDto(createdRecord));
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
