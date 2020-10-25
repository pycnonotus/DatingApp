using API.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers {
    public class BuggyController : BaseApiController {
        private readonly DataContext context;
        public BuggyController (DataContext context) {
            this.context = context;
        }

        [HttpGet ("auth")]
        public ActionResult<string> GetSecret () {
            return "Secret text";
        }
    }
}
