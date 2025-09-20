using System.ComponentModel.DataAnnotations;

namespace Koop.WebAPI.DTOs.Vehicle
{
    // Ortak kullaným için DTO
    public class SetActiveDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}