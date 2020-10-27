using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
    public class BuggyController : BaseApiController {
        private readonly DataContext context;
        public BuggyController (DataContext context) {
            this.context = context;
        }

        [Authorize]
        [HttpGet ("auth")]
        public ActionResult<string> GetSecret () => "Secret text";
        [HttpGet ("not-found")]
        public ActionResult<AppUsers> GetNotFound () {
            var thing = this.context.Users.Find (-1);
            return NotFound (); // there can't be a use with id == -1 , not logical

        }

        [HttpGet ("server-error")]
        public ActionResult<AppUsers> GetServerError () =>
            throw new InvalidOperationException (" this is a dummy method for getting am server error all is sabba");
        [HttpGet ("bad-request")]
        public ActionResult<string> GetBadRequest () => BadRequest ("This is a dummy method for getting bad request all is sabba");
    }
}
