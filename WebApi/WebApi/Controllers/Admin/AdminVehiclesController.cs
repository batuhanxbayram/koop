using Koop.Data.Context;
using Koop.Entity.DTOs.Vehicle;
using Koop.Entity.Entities; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Route("api/admin/vehicles")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminVehiclesController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminVehiclesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("{routeId}/move-first-to-end")]
    public async Task<IActionResult> MoveFirstToEnd(long routeId)
    {
        var firstInQueue = await _context.RouteVehicleQueues
            .Where(q => q.RouteId == routeId)
            .OrderBy(q => q.QueueTimestamp)
            .FirstOrDefaultAsync();

        if (firstInQueue == null)
        {
            return NotFound(new { Message = $"Bu güzergahta ({routeId}) sırada bekleyen araç bulunamadı." });
        }
        firstInQueue.QueueTimestamp = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(firstInQueue);
    }


    [HttpGet]
    public async Task<IActionResult> GetAllVehicles()
    {
        var vehicles = await _context.Vehicles
            .Include(v => v.AppUser)
            .Select(v => new VehicleDto
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                DriverName = v.AppUser.FullName,
                PhoneNumber = v.PhoneNumber,
                IsActive = v.IsActive,
                AppUserId = v.AppUserId,
                
            })
            .ToListAsync();

        return Ok(vehicles);
    }


    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto createVehicleDto)
    {
        // 1. Plaka kontrolü
        if (await _context.Vehicles.AnyAsync(v => v.LicensePlate == createVehicleDto.LicensePlate))
        {
            // --- DEĞİŞİKLİK BURADA ---
            // Hata mesajını düz string yerine bir JSON nesnesi olarak döndür.
            return BadRequest(new { Message = "Bu plaka zaten kayıtlı." });
        }

        // 2. İlişkili kullanıcıyı bul
        var user = await _context.Users.FindAsync(createVehicleDto.AppUserId);
        if (user == null)
        {
            // Bu hatayı da JSON formatına çevirelim
            return BadRequest(new { Message = "Belirtilen kullanıcı bulunamadı." });
        }

        // 3. Yeni Vehicle entity'si oluştur
        var vehicle = new Vehicle
        {
            AppUserId = createVehicleDto.AppUserId,
            LicensePlate = createVehicleDto.LicensePlate,
            DriverName = user.FullName,
            PhoneNumber = createVehicleDto.PhoneNumber,
            IsActive = true
        };

        // 4. Veritabanına ekle ve kaydet
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // 5. Frontend'e döndürülecek DTO'yu hazırla
        var vehicleDto = new VehicleDto
        {
            Id = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            DriverName = user.FullName,
            PhoneNumber = vehicle.PhoneNumber,
            IsActive = vehicle.IsActive,
            AppUserId = vehicle.AppUserId,
            UserFullName = user.FullName
        };

        // 6. Başarılı (201 Created) yanıtı DTO ile birlikte döndür
        return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, vehicleDto);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(long id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound();
        }
        return Ok(vehicle);
    }

    [HttpPost("move-to-end")]
    public async Task<IActionResult> MoveVehicleToEnd(long routeId, [FromBody] MoveVehicleDto dto)
    {
        var queueEntry = await _context.RouteVehicleQueues
            .FirstOrDefaultAsync(q => q.RouteId == routeId && q.VehicleId == dto.VehicleId);

        if (queueEntry == null) return NotFound("Araç bu sırada bulunamadı.");

        queueEntry.QueueTimestamp = DateTime.UtcNow; // Zaman damgasını şimdiki zamana ayarla
        await _context.SaveChangesAsync();
        return Ok();
    }

    // Belirtilen bir aracı sıranın en BAŞINA gönderir
    [HttpPost("move-to-front")]
    public async Task<IActionResult> MoveVehicleToFront(long routeId, [FromBody] MoveVehicleDto dto)
    {
        var queueEntry = await _context.RouteVehicleQueues
            .FirstOrDefaultAsync(q => q.RouteId == routeId && q.VehicleId == dto.VehicleId);

        if (queueEntry == null) return NotFound("Araç bu sırada bulunamadı.");

        // Sırada başka araç varsa, en öndekinin zamanından 1 saniye öncesine ayarla
        var firstInQueue = await _context.RouteVehicleQueues
            .Where(q => q.RouteId == routeId)
            .OrderBy(q => q.QueueTimestamp)
            .FirstOrDefaultAsync();

        if (firstInQueue != null && firstInQueue.VehicleId != dto.VehicleId)
        {
            queueEntry.QueueTimestamp = firstInQueue.QueueTimestamp.AddSeconds(-1);
        }
        // Sırada başka araç yoksa veya zaten ilk sıradaysa bir şey yapmaya gerek yok.

        await _context.SaveChangesAsync();
        return Ok();
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(long id, [FromBody] UpdateVehicleDto updateVehicleDto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound("Güncellenecek araç bulunamadı.");
        }
        vehicle.LicensePlate = updateVehicleDto.LicensePlate;
        vehicle.PhoneNumber = updateVehicleDto.PhoneNumber;
        vehicle.IsActive = updateVehicleDto.IsActive;
        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpPatch("{id}/set-active")]
    public async Task<IActionResult> SetVehicleActiveStatus(long id, [FromBody] SetActiveDto setActiveDto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound("Araç bulunamadı.");
        }

        vehicle.IsActive = setActiveDto.IsActive;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("idle-warnings")]
    public async Task<IActionResult> GetIdleVehicles([FromQuery] int days = 7)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-days);

   

        var idleVehicles = await _context.Vehicles
            .Include(v => v.AppUser)
            .Include(v => v.RouteVehicleQueues) 
            .Where(v => v.IsActive) 
            .Select(v => new
            {
                Vehicle = v,
                // Aracın bulunduğu en son kuyruk kaydını al (Bir araç birden fazla kuyrukta olabilir mi bilmiyoruz ama en yenisini alalım)
                LastQueueInfo = v.RouteVehicleQueues.OrderByDescending(q => q.QueueTimestamp).FirstOrDefault()
            })
            .Where(x =>
                // DURUM 1: Araç hiçbir kuyrukta yoksa (Sistemde kayıtlı ama işe çıkmamış)
                x.LastQueueInfo == null
                ||
                // DURUM 2: Kuyrukta var ama tarihi eşik değerden eski (7 gündür sırası değişmemiş)
                x.LastQueueInfo.QueueTimestamp < thresholdDate
            )
            .Select(x => new VehicleDto
            {
                Id = x.Vehicle.Id,
                LicensePlate = x.Vehicle.LicensePlate,
                DriverName = x.Vehicle.AppUser != null ? x.Vehicle.AppUser.FullName : x.Vehicle.DriverName,
                PhoneNumber = x.Vehicle.PhoneNumber,
                IsActive = x.Vehicle.IsActive,
                AppUserId = x.Vehicle.AppUserId,
                UserFullName = x.Vehicle.AppUser != null ? x.Vehicle.AppUser.FullName : ""
            })
            .ToListAsync();

        return Ok(idleVehicles);
    }



    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(long id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound("Silinecek araç bulunamadı.");
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}