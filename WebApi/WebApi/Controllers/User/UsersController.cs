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
        public UsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
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