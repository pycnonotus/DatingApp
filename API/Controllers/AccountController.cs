using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;
        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            this.mapper = mapper;
            this.tokenService = tokenService;
            this.context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {

            
            if (await UserExists(registerDto.Username.ToLower()))
            {
                return BadRequest("User already exists");
            }
            var user = this.mapper.Map<AppUsers>(registerDto);
            using var hmac = new HMACSHA512();

            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
            this.context.Users.Add(user);
            await context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await context.Users
            .Include(p => p.Photos).
            SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            if (user == null)
            {
                return Unauthorized("Cant find user with the name");
            }

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            for (int i = 0; i < hashedPassword.Length; i++)
            {
                if (hashedPassword[i] != user.PasswordHash[i])
                {
                    return Unauthorized("Invalid password");

                }
            }
            return new UserDto
            {
                Username = user.UserName,
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos?.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await this.context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
