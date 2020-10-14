using System.Collections.Generic;
using System.Linq;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
// TODO doc this shit
namespace API.Controllers {
    [ApiController]
    [Route ("/api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly DataContext context;
        public UsersController (DataContext context) {
            this.context = context;
        }

        [HttpGet]
        public ActionResult<IEnumerable<AppUsers>> GetUsers () {
            var users = context.Users.ToList ();
            return users;
        }

        /// <summary>
        /// Get a  user by an id
        /// </summary>
        [HttpGet ("{id}")]
        public ActionResult<AppUsers> GetUser (int id) {
            return context.Users.Find (id);
        }

    }
}
