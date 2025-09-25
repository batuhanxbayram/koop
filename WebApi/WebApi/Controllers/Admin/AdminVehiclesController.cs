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
                DriverName = v.DriverName,
                PhoneNumber = v.PhoneNumber,
                IsActive = v.IsActive,
                AppUserId = v.AppUserId,
                UserFullName = v.AppUser.FullName 
            })
            .ToListAsync();

        return Ok(vehicles);
    }


 
    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto createVehicleDto)
    {
        if (await _context.Vehicles.AnyAsync(v => v.LicensePlate == createVehicleDto.LicensePlate))
        {
            return BadRequest("Bu plaka zaten kayıtlı.");
        }

        var user = await _context.Users.FindAsync(createVehicleDto.AppUserId);
        if (user == null)
        {
            return BadRequest("Belirtilen kullanıcı bulunamadı.");
        }

        var vehicle = new Vehicle
        {
            AppUserId = createVehicleDto.AppUserId,
            LicensePlate = createVehicleDto.LicensePlate,
            DriverName = createVehicleDto.DriverName,
            PhoneNumber = createVehicleDto.PhoneNumber,
            IsActive = true
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        // --- DEĞİŞİKLİK BURADA BAŞLIYOR ---

        // Frontend'in ihtiyacı olan VehicleDto'yu oluşturuyoruz.
        // Bu DTO, kullanıcının tam adını da içerir.
        var vehicleDto = new VehicleDto
        {
            Id = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            DriverName = vehicle.DriverName,
            PhoneNumber = vehicle.PhoneNumber,
            IsActive = vehicle.IsActive,
            AppUserId = vehicle.AppUserId,
            UserFullName = user.FullName // Kullanıcının tam adını ekliyoruz
        };

        // Oluşturulan DTO'yu CreatedAtAction ile geri dönüyoruz.
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



    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(long id, [FromBody] UpdateVehicleDto updateVehicleDto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound("Güncellenecek araç bulunamadı.");
        }

        vehicle.LicensePlate = updateVehicleDto.LicensePlate;
        vehicle.DriverName = updateVehicleDto.DriverName;
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