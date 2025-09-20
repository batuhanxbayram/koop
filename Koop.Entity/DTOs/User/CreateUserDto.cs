using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koop.Entity.DTOs.User
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.")]
        public string UserName { get; set; }

        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }

        // Opsiyonel: Kullanıcı oluşturulurken direkt rol ataması yapmak isterseniz.
        public IList<string>? Roles { get; set; }
    }
}
