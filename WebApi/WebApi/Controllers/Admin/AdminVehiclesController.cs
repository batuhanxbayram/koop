
using Koop.Data.Context;
using Koop.Entity.DTOs.Vehicle;
using Koop.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // <-- YENİ EKLENDİ
using Microsoft.EntityFrameworkCore;
using WebApi.Hubs;

[Route("api/admin/vehicles")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminVehiclesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<QueueHub> _hubContext; // <-- YENİ EKLENDİ

    public AdminVehiclesController(AppDbContext context, IHubContext<QueueHub> hubContext) // <-- YENİ EKLENDİ
    {
        _context = context;
        _hubContext = hubContext; // <-- YENİ EKLENDİ
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

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

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
        if (await _context.Vehicles.AnyAsync(v => v.LicensePlate == createVehicleDto.LicensePlate))
        {
            return BadRequest(new { Message = "Bu plaka zaten kayıtlı." });
        }

        var user = await _context.Users.FindAsync(createVehicleDto.AppUserId);
        if (user == null)
        {
            return BadRequest(new { Message = "Belirtilen kullanıcı bulunamadı." });
        }

        var vehicle = new Vehicle
        {
            AppUserId = createVehicleDto.AppUserId,
            LicensePlate = createVehicleDto.LicensePlate,
            DriverName = user.FullName,
            PhoneNumber = createVehicleDto.PhoneNumber,
            IsActive = true
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

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

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

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

        queueEntry.QueueTimestamp = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

        return Ok();
    }

    [HttpPost("move-to-front")]
    public async Task<IActionResult> MoveVehicleToFront(long routeId, [FromBody] MoveVehicleDto dto)
    {
        var queueEntry = await _context.RouteVehicleQueues
            .FirstOrDefaultAsync(q => q.RouteId == routeId && q.VehicleId == dto.VehicleId);

        if (queueEntry == null) return NotFound("Araç bu sırada bulunamadı.");

        var firstInQueue = await _context.RouteVehicleQueues
            .Where(q => q.RouteId == routeId)
            .OrderBy(q => q.QueueTimestamp)
            .FirstOrDefaultAsync();

        if (firstInQueue != null && firstInQueue.VehicleId != dto.VehicleId)
        {
            queueEntry.QueueTimestamp = firstInQueue.QueueTimestamp.AddSeconds(-1);
        }

        await _context.SaveChangesAsync();

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

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

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

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

        // --- SİNYAL GÖNDER ---
        // Bu çok önemli, araç pasife alınınca TV'den anında düşmeli
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

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
                LastQueueInfo = v.RouteVehicleQueues.OrderByDescending(q => q.QueueTimestamp).FirstOrDefault()
            })
            .Where(x =>
                x.LastQueueInfo == null || x.LastQueueInfo.QueueTimestamp < thresholdDate
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

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

        return NoContent();
    }
}