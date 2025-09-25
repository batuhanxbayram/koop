using Koop.Data.Context;
using Koop.Entity.DTOs.User;
using Koop.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext context;

        public UsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            this.context = context;
        }


        [HttpGet]
        // [Authorize(Roles = "Admin")] // Sadece adminlerin erişmesini isterseniz bunu ekleyin
        public async Task<IActionResult> GetUserAndVehicle()
        {
            var users = await context.Users
                .Include(u => u.Vehicle) 
                .Select(u => new UserVehicleDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    LicensePlate = u.Vehicle != null ? u.Vehicle.LicensePlate : "-",
                    PhoneNumber = u.Vehicle != null ? u.Vehicle.PhoneNumber : "-"
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("without-vehicle")]
        public async Task<IActionResult> GetUserAndVehicleWithout()
        {
            var users = await context.Users
                .Include(u => u.Vehicle)
               
                .Where(u => u.Vehicle == null)
                .Select(u => new UserVehicleDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    LicensePlate = "-",
                    PhoneNumber = "-"
                })
                .ToListAsync();
            return Ok(users);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet("with-roles")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            return Ok(userDtos);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Sadece adminlerin kullanıcı silebilmesini sağlar
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            // Kullanıcıyı ID'sine göre bul
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                // Kullanıcı bulunamazsa 404 Not Found hatası döndür
                return NotFound(new { Message = "Silinecek kullanıcı bulunamadı." });
            }

            // Kullanıcıyı sil
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // Silme işlemi başarılıysa 200 OK veya 204 No Content döndür
                return Ok(new { Message = "Kullanıcı başarıyla silindi." });
            }

            // Silme işlemi sırasında bir hata oluşursa 400 Bad Request döndür
            return BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("Güncellenecek kullanıcı bulunamadı.");
            }

            // 1. Kullanıcı Adını Güncelleme
            // Eğer gelen kullanıcı adı mevcut kullanıcı adından farklıysa
            if (user.UserName != updateUserDto.UserName)
            {
                // Yeni kullanıcı adının başkası tarafından kullanılıp kullanılmadığını kontrol et
                var userExists = await _userManager.FindByNameAsync(updateUserDto.UserName);
                if (userExists != null)
                {
                    return BadRequest("Bu kullanıcı adı zaten başka bir kullanıcı tarafından alınmış.");
                }
                // UserManager ile kullanıcı adını güvenli bir şekilde güncelle
                var setUserNameResult = await _userManager.SetUserNameAsync(user, updateUserDto.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    return BadRequest(setUserNameResult.Errors);
                }
            }

            // 2. Tam Adı Güncelleme
            user.FullName = updateUserDto.FullName;

            // 3. Şifreyi Güncelleme (Eğer şifre girildiyse)
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                // Identity'nin güvenli şifre sıfırlama mekanizmasını kullanıyoruz
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, updateUserDto.Password);
                if (!resetPasswordResult.Succeeded)
                {
                    return BadRequest(resetPasswordResult.Errors);
                }
            }

            // 4. Genel Kullanıcı Güncelleme
            // FullName gibi Identity'nin temelinde olmayan alanları güncellemek için bu gerekli.
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent(); // Başarılı
            }

            return BadRequest(result.Errors);
        }


        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var userExists = await _userManager.FindByNameAsync(createUserDto.UserName);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status409Conflict, "Bu kullanıcı adı zaten alınmış.");
            }

            var user = new AppUser
            {
                UserName = createUserDto.UserName,
                FullName = createUserDto.FullName
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // --- HİBRİT ROL ATAMA MANTIĞI ---

            // EĞER frontend veya seed script'i bir rol listesi gönderdiyse:
            if (createUserDto.Roles != null && createUserDto.Roles.Any())
            {
                // Gelen rollerin sistemde var olup olmadığını kontrol et (güvenlik için)
                foreach (var roleName in createUserDto.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.DeleteAsync(user); // Hatalı durumda kullanıcıyı geri sil
                        return BadRequest($"'{roleName}' adında bir rol bulunamadı.");
                    }
                }
                // Rolleri ata
                await _userManager.AddToRolesAsync(user, createUserDto.Roles);
            }
            // EĞER frontend'den bir rol listesi GELMEDİYSE (bizim senaryomuz):
            else
            {
                var defaultRole = "User";
                if (await _roleManager.RoleExistsAsync(defaultRole))
                {
                    // Varsayılan 'User' rolünü ata
                    await _userManager.AddToRoleAsync(user, defaultRole);
                }
                else
                {
                    await _userManager.DeleteAsync(user);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Sistem hatası: Varsayılan 'User' rolü bulunamadı.");
                }
            }
            // --- MANTIK BİTTİ ---

            var createdUserDto = new UserVehicleDto { /* ... */ };
            return Ok(createdUserDto);
        }
    }
}