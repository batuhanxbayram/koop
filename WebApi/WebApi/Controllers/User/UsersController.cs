using Koop.Data.Context;
using Koop.Entity.DTOs.User;
using Koop.Entity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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


        
        [HttpPost("change-my-password")]
        [Authorize] 
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordDto dto)
        {
            // Token'dan kullanıcı ID'sini al — dışarıdan gelen ID'ye güvenme
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Mevcut şifreyi doğrula
            var isOldPasswordCorrect = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
            if (!isOldPasswordCorrect)
                return BadRequest(new { message = "Mevcut şifre hatalı." });

            // Yeni şifreyi set et
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(new { message = result.Errors.FirstOrDefault()?.Description });

            return Ok(new { message = "Şifre başarıyla güncellendi." });
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
        [Authorize(Roles = "Admin")] 
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

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var vehicle = await context.Vehicles
                .Where(v => v.AppUserId == user.Id)
                .OrderBy(v => v.Id)
                .FirstOrDefaultAsync();

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                userName = user.UserName,
                phoneNumber = user.PhoneNumber,
                licensePlate = vehicle != null ? vehicle.LicensePlate : "-"
            });
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
