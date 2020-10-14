using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// TODO doc this shit..2323
namespace API.Controllers {
    [ApiController]
    [Route ("/api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly DataContext context;
        public UsersController (DataContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUsers>>> GetUsers () {
            return await context.Users.ToListAsync ();
        }

        /// <summary>
        /// Get a  user by an id
        /// </summary>
        [HttpGet ("{id}")]
        public async Task<ActionResult<AppUsers>> GetUser (int id) {
            return await context.Users.FindAsync (id);
        }

    }
}
