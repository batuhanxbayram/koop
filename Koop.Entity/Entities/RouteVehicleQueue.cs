using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.Entities
{
    public class RouteVehicleQueue
    {
        [Key]
        public long Id { get; set; }

        public long RouteId { get; set; }
        public long VehicleId { get; set; }
        public DateTime QueueTimestamp { get; set; } = DateTime.UtcNow;
        [ForeignKey("RouteId")]
        public Route Route { get; set; } = null!;
        [ForeignKey("VehicleId")] 
        public Vehicle Vehicle { get; set; } = null!;
    }

}
