using Koop.Data.Context;
using Koop.Entity.DTOs.Vehicle;
using Koop.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers.Admin
{
    [Route("api/routes/{routeId}/queue")] 
    [ApiController]
    
    public class RouteQueueController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RouteQueueController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("/api/queues/all")] 
        public async Task<IActionResult> GetAllQueues()
        {
            
            var allRoutes = await _context.Routes.Where(r => r.IsActive).OrderBy(r => r.RouteName).ToListAsync();

            var result = new List<RouteWithQueueDto>();

  
            foreach (var route in allRoutes)
            {
                var queuedVehicles = await _context.RouteVehicleQueues
                    .Where(q => q.RouteId == route.Id)
                    .OrderBy(q => q.QueueTimestamp)
                    .Include(q => q.Vehicle.AppUser)
                    .Select(q => new VehicleDto
                    {
                        Id = q.Vehicle.Id,
                        LicensePlate = q.Vehicle.LicensePlate,
                        UserFullName = q.Vehicle.AppUser.FullName,
                       
                    })
                    .ToListAsync();

                result.Add(new RouteWithQueueDto
                {
                    RouteId = route.Id,
                    RouteName = route.RouteName,
                    QueuedVehicles = queuedVehicles
                });
            }

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> GetQueueForRoute(long routeId)
        {
            var queuedVehicles = await _context.RouteVehicleQueues
                .Where(q => q.RouteId == routeId)
                .OrderBy(q => q.QueueTimestamp) // En eski tarihli olan en üstte (FIFO)
                .Include(q => q.Vehicle.AppUser) // Aracın ve kullanıcının bilgilerini de al
                .Select(q => new VehicleDto // Bildiğimiz VehicleDto'yu kullanalım
                {
                    Id = q.Vehicle.Id,
                    LicensePlate = q.Vehicle.LicensePlate,
                    UserFullName = q.Vehicle.AppUser.FullName,
                    PhoneNumber = q.Vehicle.PhoneNumber,
                    DriverName = q.Vehicle.DriverName,
                    IsActive = q.Vehicle.IsActive,
                    AppUserId = q.Vehicle.AppUserId
                })
                .ToListAsync();

            return Ok(queuedVehicles);
        }

    
        [HttpPost]
        public async Task<IActionResult> AddVehicleToQueue(long routeId, [FromBody] AddVehicleToQueueDto dto)
        {
            // 1. Güzergah var mı?
            var routeExists = await _context.Routes.AnyAsync(r => r.Id == routeId);
            if (!routeExists) return NotFound("Güzergah bulunamadı.");

            // 2. Araç var mı?
            var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == dto.VehicleId);
            if (!vehicleExists) return NotFound("Araç bulunamadı.");

            // 3. Bu araç zaten bu sırada var mı? (Veritabanı UNIQUE kısıtlamanız bunu zaten engeller ama bu daha temiz bir hata mesajı verir)
            var alreadyInQueue = await _context.RouteVehicleQueues.AnyAsync(q => q.RouteId == routeId && q.VehicleId == dto.VehicleId);
            if (alreadyInQueue) return BadRequest("Bu araç zaten bu sırada mevcut.");

            var queueEntry = new RouteVehicleQueue
            {
                RouteId = routeId,
                VehicleId = dto.VehicleId,
                QueueTimestamp = DateTime.UtcNow // Sıraya giriş anı
            };

            _context.RouteVehicleQueues.Add(queueEntry);
            await _context.SaveChangesAsync();

            return Ok(queueEntry); // Başarılı
        }

       
        [HttpDelete("{vehicleId}")]
        public async Task<IActionResult> RemoveVehicleFromQueue(long routeId, long vehicleId)
        {
            var queueEntry = await _context.RouteVehicleQueues.FirstOrDefaultAsync(q => q.RouteId == routeId && q.VehicleId == vehicleId);

            if (queueEntry == null)
            {
                return NotFound("Araç bu sırada bulunamadı.");
            }

            _context.RouteVehicleQueues.Remove(queueEntry);
            await _context.SaveChangesAsync();

            return NoContent(); // Başarılı silme
        }


        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderQueue(long routeId, [FromBody] ReorderQueueDto dto)
        {
         
            var queueEntries = await _context.RouteVehicleQueues
                .Where(q => q.RouteId == routeId)
                .ToListAsync();

        
            if (dto.OrderedVehicleIds.Count != queueEntries.Count)
            {
                return BadRequest("Sıralama listesi ile veritabanı kaydı eşleşmiyor.");
            }

           
            var baseTimestamp = DateTime.UtcNow;

        
            for (int i = 0; i < dto.OrderedVehicleIds.Count; i++)
            {
                var vehicleId = dto.OrderedVehicleIds[i];
                var entryToUpdate = queueEntries.FirstOrDefault(q => q.VehicleId == vehicleId);

                if (entryToUpdate != null)
                {
                    
                    entryToUpdate.QueueTimestamp = baseTimestamp.AddSeconds(i);
                }
            }

            
            await _context.SaveChangesAsync();

            return Ok("Sıralama başarıyla güncellendi.");
        }
    }
}
