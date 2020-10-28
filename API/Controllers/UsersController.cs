using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers {
    [Authorize]
    public class UsersController : BaseApiController {
        private readonly IMapper mapper;

        private readonly IUserRepository userRepository;
        public UsersController (IUserRepository userRepository, IMapper mapper) {
            this.mapper = mapper;

            this.userRepository = userRepository;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers () {
            var users = await this.userRepository.GetMembersAsync ();
            return Ok (users);
        }

        /// <summary>
        /// Get a  user by an id
        /// </summary>
        [HttpGet ("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser (string username) {
            return await this.userRepository.GetMemberAsync (username);

        }
    }
}
