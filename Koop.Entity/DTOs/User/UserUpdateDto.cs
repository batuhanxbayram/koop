using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.User
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Tam ad zorunludur.")]
        [StringLength(100)]
        public string FullName { get; set; }

       
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string? ConfirmPassword { get; set; }
    }
}
 