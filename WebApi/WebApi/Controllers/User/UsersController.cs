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
        .AsNoTracking()
        .Include(u => u.Vehicles)
        .Select(u => new UserVehicleDto
        {
            Id = u.Id,
            FullName = u.FullName,
            PhoneNumber = u.PhoneNumber ?? "-",  
            LicensePlate = u.Vehicles
                .OrderBy(v => v.Id)
                .Select(v => v.LicensePlate)
                .FirstOrDefault() ?? "-"
        })
        .ToListAsync();

            return Ok(users);
        }

        [HttpGet("without-vehicle")]
        public async Task<IActionResult> GetUserAndVehicleWithout()
        {
            var users = await context.Users
                .AsNoTracking()
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

        [HttpPost("create-pool-user")]
        public async Task<IActionResult> CreatePoolUser([FromServices] UserManager<AppUser> userManager)
        {
            // 1. Kullanıcının var olup olmadığını kontrol et (Çift kaydı önlemek için)
            var existingUser = await userManager.FindByNameAsync("sistem_havuz");
            if (existingUser != null)
            {
                return Ok(new { message = "Bu kullanıcı zaten var!", userId = existingUser.Id });
            }

            // 2. Yeni kullanıcı nesnesini oluştur
            var poolUser = new AppUser
            {
                UserName = "sistem_havuz",
                Email = "havuz@75ymkt.com", 
                EmailConfirmed = true,
                FullName = "Sistem Aktarım",
               
            };

            // 3. Kullanıcıyı güçlü bir şifre ile Identity üzerinden kaydet
            // Şifre kurallarına takılmamak için büyük/küçük harf, rakam ve özel karakter içeren bir şifre veriyoruz
            var result = await userManager.CreateAsync(poolUser, "HavuzUser.123456!");

            if (result.Succeeded)
            {
                // Başarılı olursa bize oluşturulan GUID'i (Id) dönecek
                return Ok(new
                {
                    message = "Havuz kullanıcı başarıyla oluşturuldu!",
                    userId = poolUser.Id
                });
            }

            // Hata olursa Identity hatalarını listele
            return BadRequest(result.Errors);
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

            // YALNIZCA BU SATIR KALDI: Kullanıcının tam adını güncelliyoruz.
            user.FullName = updateUserDto.FullName;

            // Şifreyi Güncelleme (Eğer şifre girildiyse)
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, updateUserDto.Password);
                if (!resetPasswordResult.Succeeded)
                {
                    return BadRequest(resetPasswordResult.Errors);
                }
            }

            // Genel Kullanıcı Güncelleme
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }


        [HttpPost]
        // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userExists = await _userManager.FindByNameAsync(createUserDto.UserName);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status409Conflict, "Bu kullanıcı adı zaten alınmış.");
            }

            var user = new AppUser
            {
                UserName = createUserDto.UserName,
                FullName = createUserDto.FullName,
                PhoneNumber = createUserDto.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, createUserDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

          
            if (createUserDto.Roles != null && createUserDto.Roles.Any())
            {
               
                foreach (var roleName in createUserDto.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _userManager.DeleteAsync(user); 
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
           

            var createdUserDto = new UserVehicleDto
            {
                Id = user.Id,
                FullName = user.FullName,
                LicensePlate = "-",
                PhoneNumber = "-"
            };
            return Ok(createdUserDto);
        }
    }
}
