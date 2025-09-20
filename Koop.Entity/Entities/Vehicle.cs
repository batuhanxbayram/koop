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

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [StringLength(150)]
        public string? DriverName { get; set; } // Soru işareti, bu alanın nullable (boş olabilir) olduğunu belirtir.

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        // İsteğiniz üzerine eklenen aktif/pasif durumu
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property: Bir aracın birden çok sıra kaydı olabilir.
        public ICollection<RouteVehicleQueue> RouteVehicleQueues { get; set; } = new List<RouteVehicleQueue>();
    }
}
