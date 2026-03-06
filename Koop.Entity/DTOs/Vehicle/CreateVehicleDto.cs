using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.Vehicle
{
    public class CreateVehicleDto
    {
        public Guid? AppUserId { get; set; } // Boş bırakılırsa araç kullanıcıya atanmadan oluşturulur.

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [StringLength(150)]
        public string? DriverName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
