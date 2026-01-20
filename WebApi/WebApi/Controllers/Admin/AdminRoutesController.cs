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

    [HttpGet]
    public async Task<IActionResult> GetAllRoutes()
    {
        var routes = await _context.Routes
            .OrderBy(r => r.RouteName)
            .ToListAsync();

        return Ok(routes);
    }
    [HttpPost("seed-test-data")]
    public async Task<IActionResult> SeedTestData()
    {
        // 1. Önce veritabanında Güzergah (Route) var mı kontrol et
        var routes = await _context.Routes.ToListAsync();
        if (!routes.Any())
            return BadRequest("Hata: Sistemde hiç güzergah yok! Önce en az 1 tane güzergah eklemelisin.");

        var random = new Random();
        int totalVehicles = 120; // Eklenecek araç sayısı

        for (int i = 1; i <= totalVehicles; i++)
        {
            // --- A. KULLANICI (ŞOFÖR) OLUŞTURMA ---
            var user = new AppUser
            {
                UserName = $"TestUser_{i}_{Guid.NewGuid().ToString().Substring(0, 4)}", // Benzersiz ID
                Email = $"testsofor{i}@koop.com",
                FullName = $"Test Şoför {i}", // BURASI ÖNEMLİ (Boş olmamalı)
                EmailConfirmed = true,
                PhoneNumber = "5551234567"
            };

            // Identity kütüphanesi ile kullanıcıyı kaydet
            var result = await userManager.CreateAsync(user, "Test.123456");

            if (result.Succeeded)
            {
                // --- B. ARAÇ (VEHICLE) OLUŞTURMA ---
                // Plaka üretimi: Örn. 41 TST 001
                string cityCode = random.Next(1, 82).ToString("00");
                string letters = "TST";
                string number = i.ToString("000");
                string licensePlate = $"{cityCode} {letters} {number}";

                var vehicle = new Vehicle
                {
                    AppUserId = user.Id,        // Kullanıcı ile ilişkilendir
                    LicensePlate = licensePlate,
                    DriverName = user.FullName, // Şoför adını buraya da yazıyoruz
                    PhoneNumber = user.PhoneNumber,
                    IsActive = true
                };

                // Aracı ekle (ID oluşması için save şart)
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                // --- C. KUYRUĞA (ROUTE VEHICLE QUEUE) EKLEME ---
                var randomRoute = routes[random.Next(routes.Count)]; // Rastgele bir güzergah seç

                var queueItem = new RouteVehicleQueue
                {
                    RouteId = randomRoute.Id,
                    VehicleId = vehicle.Id,     // Oluşan Vehicle ID'sini kullan
                    QueueTimestamp = DateTime.UtcNow.AddMinutes(-i) // Eskiden yeniye sıralı olsun diye zamanı geriye alıyoruz
                };

                _context.RouteVehicleQueues.Add(queueItem);
            }
        }

        // Kuyruk kayıtlarını topluca kaydet
        await _context.SaveChangesAsync();

        return Ok($"İşlem Başarılı: {totalVehicles} adet test aracı ve kuyruk kaydı oluşturuldu.");
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