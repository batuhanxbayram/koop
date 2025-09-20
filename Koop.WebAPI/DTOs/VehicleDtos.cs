using System.ComponentModel.DataAnnotations;

namespace Koop.WebAPI.DTOs
{
    public class CreateVehicleDto
    {
        [Required]
        public Guid AppUserId { get; set; } // Aracý atayacaðýnýz þoförün ID'si

        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [StringLength(150)]
        public string? DriverName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }    
    }

    public class UpdateVehicleDto
    {
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; }

        [StringLength(150)]
        public string? DriverName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}