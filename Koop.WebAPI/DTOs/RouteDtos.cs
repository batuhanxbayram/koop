using System.ComponentModel.DataAnnotations;

namespace Koop.WebAPI.DTOs
{
    public class CreateRouteDto
    {
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; }
    }

    public class UpdateRouteDto
    {
        [Required]
        [StringLength(100)]
        public string RouteName { get; set; }

        public bool IsActive { get; set; }
    }
}