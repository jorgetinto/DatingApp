using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userDTO)
        {
            userDTO.Username = userDTO.Username.ToLower();

            if (await _repo.UserExists(userDTO.Username))
                return BadRequest("Username alredy exists");

            var userToCreate = _mapper.Map<User>(userDTO);

            var createUser = await _repo.Register(userToCreate, userDTO.Password);

            var userToReturn = _mapper.Map<UserForDetailedDto>(createUser);

            return CreatedAtRoute("GetUser", new {Controller="Users", id = createUser.Id}, userToReturn);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserForLoginDto userDTO)
        {
            User userRepo = await _repo.Login(userDTO.UserName.ToLower(), userDTO.Password);

            if (userRepo == null)
                return Unauthorized();

            var clains = new[]{
                new Claim(ClaimTypes.NameIdentifier, userRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userRepo.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(clains),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var user = _mapper.Map<UserForListDto>(userRepo);

            return Ok(new { token = tokenHandler.WriteToken(token), user });
        }
    }
}