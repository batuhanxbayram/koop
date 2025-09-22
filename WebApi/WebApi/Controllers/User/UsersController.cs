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


        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
           
            var userExists = await _userManager.FindByNameAsync(createUserDto.UserName);
            if (userExists != null)
            {
               
                return StatusCode(StatusCodes.Status409Conflict, new { Message = "Bu kullanıcı adı zaten alınmış." });
            }

            var user = new AppUser { UserName = createUserDto.UserName , FullName=createUserDto.FullName };
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
                        return BadRequest(new { Message = $"'{roleName}' adında bir rol bulunamadı." });
                    }
                }
                await _userManager.AddToRolesAsync(user, createUserDto.Roles);
            }
            else
            {
            
                var defaultRole = "User";
                if (await _roleManager.RoleExistsAsync(defaultRole))
                {
                    await _userManager.AddToRoleAsync(user, defaultRole);
                }
                else
                {
                  
                    await _userManager.DeleteAsync(user);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Varsayılan 'User' rolü sistemde tanımlı değil. Kullanıcı oluşturulamadı." });
                }
            }
          

            var createdUserDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = await _userManager.GetRolesAsync(user)
            };

            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, createdUserDto);
        }
    }
}