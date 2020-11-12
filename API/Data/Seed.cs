using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<AppUsers> userManager)
        {
            if (await userManager.Users.AnyAsync()) return; // if we have any users
            var usersData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.Json");

            var users = JsonSerializer.Deserialize<List<AppUsers>>(usersData);

            foreach (var user in users)
            {
                // using var hmac = new HMACSHA512 ();
                user.UserName = user.UserName.ToLower();
                // user.PasswordHash = hmac.ComputeHash (Encoding.UTF8.GetBytes ("Pa$$w0rd"));
                // user.PasswordSalt = hmac.Key;

                await userManager.CreateAsync(user, "Pa$$w0rd");

            }


        }
    }
}
