
    using Koop.Data.Context;
    using Koop.Entity.DTOs.Vehicle;
    using Koop.Entity.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR; // <-- YENİ EKLENDİ
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using WebApi.Hubs;

    namespace WebApi.Controllers.Admin
    {
        [Route("api/routes/{routeId}/queue")]
        [ApiController]
        public class RouteQueueController : ControllerBase
        {
            private readonly AppDbContext _context;
            private readonly IHubContext<QueueHub> _hubContext; 

            public RouteQueueController(AppDbContext context, IHubContext<QueueHub> hubContext) 
            {
                _context = context;
                _hubContext = hubContext; 
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
                        // 🟢 DİKKAT: AppUser null ise patlamaması için kontrol eklendi
                        UserFullName = q.Vehicle.AppUser != null ? q.Vehicle.AppUser.FullName : "Atanmadı",
                        DriverName = q.Vehicle.DriverName,
                        IsActive = q.Vehicle.IsActive,
                        AppUserId = q.Vehicle.AppUserId 
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
                .OrderBy(q => q.QueueTimestamp)
                .Include(q => q.Vehicle.AppUser)
                .Select(q => new VehicleDto
                {
                    Id = q.Vehicle.Id,
                    LicensePlate = q.Vehicle.LicensePlate,
                    UserFullName = q.Vehicle.AppUser != null ? q.Vehicle.AppUser.FullName : "Atanmadı",
                    DriverName = q.Vehicle.DriverName,
                    IsActive = q.Vehicle.IsActive,
                    AppUserId = q.Vehicle.AppUserId
                })
                .ToListAsync();

            return Ok(queuedVehicles);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddVehicleToQueue(long routeId, [FromBody] AddVehicleToQueueDto dto)
        {
            var routeExists = await _context.Routes.AnyAsync(r => r.Id == routeId);
            if (!routeExists) return NotFound("Güzergah bulunamadı.");

            var vehicleExists = await _context.Vehicles.AnyAsync(v => v.Id == dto.VehicleId);
            if (!vehicleExists) return NotFound("Araç bulunamadı.");

            var alreadyInQueue = await _context.RouteVehicleQueues
                .AnyAsync(q => q.RouteId == routeId && q.VehicleId == dto.VehicleId);
            if (alreadyInQueue) return BadRequest("Bu araç zaten bu sırada mevcut.");

            // DEĞİŞİKLİK: Önce kayıt var mı kontrol et, yoksa UtcNow kullan
            DateTime newTimestamp;
            var hasEntries = await _context.RouteVehicleQueues
                .AnyAsync(q => q.RouteId == routeId);

            if (hasEntries)
            {
                var maxTimestamp = await _context.RouteVehicleQueues
                    .Where(q => q.RouteId == routeId)
                    .MaxAsync(q => q.QueueTimestamp);
                newTimestamp = maxTimestamp.AddSeconds(1);
            }
            else
            {
                newTimestamp = DateTime.UtcNow;
            }

            var queueEntry = new RouteVehicleQueue
            {
                RouteId = routeId,
                VehicleId = dto.VehicleId,
                QueueTimestamp = newTimestamp
            };

            _context.RouteVehicleQueues.Add(queueEntry);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

            return Ok(queueEntry);
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

                // --- SİNYAL GÖNDER ---
                await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

                return NoContent();
            }

        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderQueue(long routeId, [FromBody] ReorderQueueDto dto)
        {
            var queueEntries = await _context.RouteVehicleQueues
                .Where(q => q.RouteId == routeId)
                .ToListAsync();

            // Sadece gelen ID'leri işle, bilinmeyenleri atla
            var baseTimestamp = DateTime.UtcNow;
            var processed = 0;

            for (int i = 0; i < dto.OrderedVehicleIds.Count; i++)
            {
                var vehicleId = dto.OrderedVehicleIds[i];
                var entryToUpdate = queueEntries.FirstOrDefault(q => q.VehicleId == vehicleId);

                if (entryToUpdate != null)
                {
                    entryToUpdate.QueueTimestamp = baseTimestamp.AddSeconds(i);
                    processed++;
                }
            }

            if (processed == 0)
                return BadRequest("Güncellenecek geçerli araç bulunamadı.");

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

            return Ok($"{processed} araç sırası güncellendi.");
        }

        [HttpPost("move-to-end")]
            public async Task<IActionResult> MoveVehicleToEnd(long routeId, [FromBody] MoveVehicleDto dto)
            {
                var queueEntry = await _context.RouteVehicleQueues
                    .FirstOrDefaultAsync(q => q.RouteId == routeId && q.VehicleId == dto.VehicleId);

                if (queueEntry == null)
                {
                    return NotFound("Araç bu sırada bulunamadı.");
                }

                queueEntry.QueueTimestamp = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // --- SİNYAL GÖNDER ---
                await _hubContext.Clients.All.SendAsync("ReceiveQueueUpdate");

                return Ok();
            }
        }
    }