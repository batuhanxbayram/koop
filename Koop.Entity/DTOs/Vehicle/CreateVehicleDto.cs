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
        [Required]
        public Guid AppUserId { get; set; } // Aracı atayacağınız şoförün ID'si

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [StringLength(150)]
        public string? DriverName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
