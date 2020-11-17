using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        private readonly IUnitOfWork unitOfWork;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            this.unitOfWork = unitOfWork;
            this.photoService = photoService;
            this.mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = user.Gender == "Male" ? "female" : "male";
            }
            var users = await this.unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPagingHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages
            );
            return Ok(users);
        }

        /// <summary>
        /// Get a  user by an id
        /// </summary>
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await this.unitOfWork.UserRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUsername();
            var user = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            this.mapper.Map(memberUpdateDto, user);
            this.unitOfWork.UserRepository.Update(user);
            if (await this.unitOfWork.Complete()) return NoContent();
            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {

            var username = User.GetUsername();
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var res = await this.photoService.AddPhotoAsync(file);
            if (res.Error != null)
            {
                return BadRequest(res.Error.Message);
            }
            var photo = new Photo
            {
                Url = res.SecureUrl.AbsoluteUri,
                PublicId = res.PublicId
            };
            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);
            if (await this.unitOfWork.Complete())
            {

                return CreatedAtRoute("GetUser",
                    new
                    {
                        username = user.UserName
                    }, this.mapper.Map<PhotoDto>(photo));

            }
            return BadRequest("Problem Uploading Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(this.User.GetUsername());

            var photo = user.Photos.FirstOrDefault(
                x => x.Id == photoId);

            if (photo.IsMain) return BadRequest(" Photo is already set as the main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await this.unitOfWork.Complete())
            {
                return NoContent();
            }

            return BadRequest("Failed to set photo as main");

        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhotoAsync(int photoId)
        {
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(this.User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("you can't delete your main photo");
            if (photo.PublicId != null)
            {
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if (await this.unitOfWork.Complete()) return Ok();
            return BadRequest("Failed to delete the photo");
        }

    }
}
