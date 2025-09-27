using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.Vehicle
{
    public class RouteWithQueueDto
    {
        public long RouteId { get; set; }
        public string RouteName { get; set; }
        // Bu güzergahın sırasındaki araçların listesi
        public List<VehicleDto> QueuedVehicles { get; set; } = new List<VehicleDto>();
    }
}
