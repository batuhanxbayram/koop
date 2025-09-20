using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.Entities
{
    public class Route
    {
        [Key] 
        public long Id { get; set; }

        [Required] 
        [StringLength(100)] 
        public string RouteName { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<RouteVehicleQueue> RouteVehicleQueues { get; set; } = new List<RouteVehicleQueue>();
    }
}
