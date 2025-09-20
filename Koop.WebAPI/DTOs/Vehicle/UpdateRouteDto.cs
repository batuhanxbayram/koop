using System.ComponentModel.DataAnnotations;

namespace Koop.WebAPI.DTOs.Vehicle
{
    public class UpdateRouteDto
    {
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; }

        public bool IsActive { get; set; }
    }
}