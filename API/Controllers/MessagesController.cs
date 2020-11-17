using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;

        }


        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessage)
        {
            var username = User.GetUsername();

            if (username == createMessage.RecipientUsername.ToLower()) return BadRequest("you cant talk with ur self");
            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessage.RecipientUsername);
            if (recipient == null) return NotFound("recipient not found");
            var massage = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = username,
                RecipientUsername = recipient.UserName,
                Content = createMessage.Content,
            };
            unitOfWork.MessageRepository.AddMessage(massage);
            if (await unitOfWork.Complete()) return Ok(
                mapper.Map<MessageDto>(massage)
            );

            return BadRequest("Could not send the message u wanker ");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await this.unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
            Response.AddPagingHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return messages;
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUserName = User.GetUsername();
            var message = await this.unitOfWork.MessageRepository.GetMessage(id);
            if (message.Sender.UserName != currentUserName && message.Recipient.UserName != currentUserName)
            {
                return Unauthorized();
            }
            if (message.Sender.UserName == currentUserName) message.SenderDeleted = true;
            if (message.Recipient.UserName == currentUserName) message.RecipientDeleted = true;
            if (message.RecipientDeleted && message.SenderDeleted) this.unitOfWork.MessageRepository.DeleteMessage(message);
            return await this.unitOfWork.Complete() ? Ok() : BadRequest(" Problem Deleting Message");

        }





    }
}
