using Koop.Entity.DTOs.Auth;
using Koop.Entity.Entities;
using Koop.Service.Services.TokenService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        //{
        //    var userExists = await _userManager.FindByNameAsync(registerDto.Username);
        //    if (userExists != null)
        //    {
        //        return StatusCode(StatusCodes.Status409Conflict, new { Message = "Bu kullanıcı adı zaten alınmış." });
        //    }

        //    AppUser newUser = new()
        //    {
        //        UserName = registerDto.Username,
        //        Email = registerDto.Email,
        //        FullName = registerDto.FullName,
        //        SecurityStamp = Guid.NewGuid().ToString()
        //    };

        //    var result = await _userManager.CreateAsync(newUser, registerDto.Password);

        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(new { Message = "Kullanıcı oluşturulurken bir hata oluştu.", Errors = result.Errors });
        //    }

        //    return StatusCode(StatusCodes.Status201Created, new { Message = "Kullanıcı başarıyla oluşturuldu." });
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(new { Message = "Kullanıcı adı veya şifre hatalı." });
            }

            // GÜNCELLEME: Tek satırda servisi çağırıp sonucu alıyoruz.
            var tokenResponse = await _tokenService.CreateTokenStringAsync(user);

            return Ok(tokenResponse);
        }
    }
}
