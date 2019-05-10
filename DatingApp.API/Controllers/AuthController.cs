using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _config = config;
            _repo = repo;

        }


        [HttpGet]
        public string data()
        {
            return "vijay";
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("User already exists");

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username.ToLower()
            };

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();

            var claims = new[]
            {
                  new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                  new Claim(ClaimTypes.Name,userFromRepo.Username)
             };


            var key = new SymmetricSecurityKey(Encoding.ASCII
                    .GetBytes(_config.GetSection("AppSettings:SecretKey").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = creds,
                Expires = DateTime.Now.AddDays(1)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }

    }
}