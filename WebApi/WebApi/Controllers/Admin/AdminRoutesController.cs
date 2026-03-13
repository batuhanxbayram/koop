using Koop.Data.Context;
using Koop.Entity.DTOs.Vehicle;
using Koop.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR; // <-- YENİ EKLENDİ
using Microsoft.EntityFrameworkCore;
using WebApi.Hubs;

[Route("api/admin/routes")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminRoutesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<QueueHub> _hubContext; // <-- YENİ EKLENDİ
    private readonly UserManager<AppUser> userManager;

    public AdminRoutesController(AppDbContext context, IHubContext<QueueHub> hubContext,UserManager<AppUser> userManager) // <-- YENİ EKLENDİ
    {
        _context = context;
        _hubContext = hubContext; // <-- YENİ EKLENDİ
        this.userManager = userManager;
    }
    // POST: api/routes/{routeId}/import-queue
    [HttpPost("{routeId}/import-queue")]
    public async Task<IActionResult> ImportQueue(int routeId, [FromBody] ImportQueueDto request)
    {
        // Havuz kullanıcımızın ID'si (Veritabanından baktığın ID'yi buraya yaz)
        int havuzUserId = 1;

        // 1. Plakadaki boşlukları silip büyük harfe çevirelim (Eşleşme kolay olsun)
        var cleanedPlate = request.LicensePlate.Replace(" ", "").ToUpper();

        // 2. Bu plaka sistemimizde kayıtlı mı?
        var vehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.LicensePlate.Replace(" ", "").ToUpper() == cleanedPlate);

        // 3. Sistemde kayıtlı değilse, YOK SAYMA! Havuz kullanıcıya bağlayarak oluştur.
        if (vehicle == null)
        {
            vehicle = new Vehicle
            {
                LicensePlate = request.LicensePlate, // Orijinal halini kaydet (Örn: 41 ABC 123)
                Id = havuzUserId
            };
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync(); // Araç artık sistemde var ve ID'si oluştu
        }

        // 4. Aracı ilgili güzergahın kuyruğuna ekle
        var queueItem = new RouteVehicleQueue
        {
            RouteId = routeId,
            VehicleId = vehicle.Id,
            // İstersen request'ten gelen "Sira" bilgisini de buraya kaydedebilirsin
        };

        _context.RouteVehicleQueues.Add(queueItem);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{request.LicensePlate} başarıyla eklendi." });
    }

    // DTO Sınıfı
    public class ImportQueueDto
    {
        public string LicensePlate { get; set; }
        public int Sira { get; set; }
    }


    [HttpGet]
    public async Task<IActionResult> GetAllRoutes()
    {
        var routes = await _context.Routes
            .OrderBy(r => r.RouteName)
            .ToListAsync();

        return Ok(routes);
    }
    


    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDto createRouteDto)
    {
        if (await _context.Routes.AnyAsync(r => r.RouteName == createRouteDto.RouteName))
        {
            return BadRequest("Bu rota adı zaten mevcut.");
        }

        var route = new Koop.Entity.Entities.Route
        {
            RouteName = createRouteDto.RouteName,
            IsActive = true
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync();

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

        return CreatedAtAction(nameof(GetRouteById), new { id = route.Id }, route);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteById(long id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound();
        }
        return Ok(route);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(long id, [FromBody] UpdateRouteDto updateRouteDto)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound("Güncellenecek rota bulunamadı.");
        }

        route.RouteName = updateRouteDto.RouteName;
        route.IsActive = updateRouteDto.IsActive;

        await _context.SaveChangesAsync();

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

        return NoContent();
    }

    [HttpPatch("{id}/set-active")]
    public async Task<IActionResult> SetRouteActiveStatus(long id, [FromBody] SetActiveDto setActiveDto)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound("Rota bulunamadı.");
        }

        route.IsActive = setActiveDto.IsActive;
        await _context.SaveChangesAsync();

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(long id)
    {
        var route = await _context.Routes.FindAsync(id);
        if (route == null)
        {
            return NotFound("Silinecek rota bulunamadı.");
        }
        _context.Routes.Remove(route);
        await _context.SaveChangesAsync();

        // --- SİNYAL GÖNDER ---
        await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

        return NoContent();
    }
}