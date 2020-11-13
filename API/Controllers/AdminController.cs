using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUsers> userManager;
        public AdminController(UserManager<AppUsers> userManager)
        {
            this.userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await this.userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(r => r.Role)
            .OrderBy(o => o.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            }
            )
            .ToListAsync();

            return Ok(users);
        }

        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",").ToArray();
            var user = await this.userManager.FindByNameAsync(username);
            if (user == null) return NotFound("User not found");

            var userRoles = await this.userManager.GetRolesAsync(user);
            var results = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!results.Succeeded) BadRequest("Failed to add roles");
            results = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!results.Succeeded) BadRequest("Failed to remove roles");
            return Ok(
                await this.userManager.GetRolesAsync(user)
            );

        }



        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("if you see this you are an admin or a moderator");
        }

    }
}
