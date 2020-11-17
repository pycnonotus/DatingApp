using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikeController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;

        public LikeController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;

        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await this.unitOfWork.LikeRepository.GetUserWithLikes(sourceUserId);
            if (likedUser == null) return NotFound();
            if (sourceUser.UserName == username) return BadRequest(" U can't like ur self bitch");
            var userLike = await this.unitOfWork.LikeRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest(" u arelady liked this ass");
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id,

            };
            sourceUser.LikedUsers.Add(userLike);
            if (await unitOfWork.Complete())
            {
                return Ok();
            }
            return BadRequest(" error on like");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(
            [FromQuery]
            LikesParams likeParams)
        {
            likeParams.UserId = User.GetUserId();
            var users = await this.unitOfWork.LikeRepository.GetUserLikes(likeParams);

            Response.AddPagingHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

    }
}
