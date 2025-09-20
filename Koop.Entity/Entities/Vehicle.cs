using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.Entities
{
    public class Vehicle
    {
        [Key]
        public long Id { get; set; }

        public Guid AppUserId { get; set; }

        public AppUser AppUser { get; set; }

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [StringLength(150)]
        public string? DriverName { get; set; } 

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        // İsteğiniz üzerine eklenen aktif/pasif durumu
        public bool IsActive { get; set; } = true;

        public ICollection<RouteVehicleQueue> RouteVehicleQueues { get; set; } = new List<RouteVehicleQueue>();
    }
}
