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

    /// <summary>
    /// Yeni bir araç ekler.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto createVehicleDto)
    {
        // Aynı plakada başka bir araç var mı diye kontrol et
        if (await _context.Vehicles.AnyAsync(v => v.LicensePlate == createVehicleDto.LicensePlate))
        {
            return BadRequest("Bu plaka zaten kayıtlı.");
        }

        // Atanmak istenen kullanıcı var mı diye kontrol et
        var userExists = await _context.Users.AnyAsync(u => u.Id == createVehicleDto.AppUserId);
        if (!userExists)
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

        return CreatedAtAction(nameof(GetVehicleById), new { id = vehicle.Id }, vehicle);
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