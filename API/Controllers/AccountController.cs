using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers {
    public class AccountController : BaseApiController {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        public AccountController (DataContext context, ITokenService tokenService) {
            this.tokenService = tokenService;
            this.context = context;
        }

        [HttpPost ("register")]
        public async Task<ActionResult<UserDto>> Register (RegisterDto registerDto) {

            if (await UserExists (registerDto.Username.ToLower ())) {
                return BadRequest ("User already exists");
            }

            using var hmac = new HMACSHA512 ();
            var user = new AppUsers () {
                UserName = registerDto.Username.ToLower (),
                PasswordHash = hmac.ComputeHash (Encoding.UTF8.GetBytes (registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            this.context.Users.Add (user);
            await context.SaveChangesAsync ();

            return new UserDto {
                Username = user.UserName,
                    Token = tokenService.CreateToken (user)
            };
        }

        [HttpPost ("login")]
        public async Task<ActionResult<UserDto>> Login (LoginDto loginDto) {
            var user = await context.Users.SingleOrDefaultAsync (x => x.UserName == loginDto.Username.ToLower ());
            if (user == null) {
                return Unauthorized ("Cant find user with the name");
            }

            using var hmac = new HMACSHA512 (user.PasswordSalt);
            var hashedPassword = hmac.ComputeHash (Encoding.UTF8.GetBytes (loginDto.Password));
            for (int i = 0; i < hashedPassword.Length; i++) {
                if (hashedPassword[i] != user.PasswordHash[i]) {
                    return Unauthorized ("Invalid password");

                }
            }
            return new UserDto {
                Username = user.UserName,
                    Token = tokenService.CreateToken (user)
            };
        }
        private async Task<bool> UserExists (string username) {
            return await this.context.Users.AnyAsync (x => x.UserName == username.ToLower ());
        }
    }
}
