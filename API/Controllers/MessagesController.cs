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
        private readonly IUserRepository userRepository;
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            this.mapper = mapper;
            this.messageRepository = messageRepository;
            this.userRepository = userRepository;
        }


        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessage)
        {
            var username = User.GetUsername();

            if (username == createMessage.RecipientUsername.ToLower()) return BadRequest("you cant talk with ur self");
            var sender = await userRepository.GetUserByUsernameAsync(username);
            var recipient = await userRepository.GetUserByUsernameAsync(createMessage.RecipientUsername);
            if (recipient == null) return NotFound("recipient not found");
            var massage = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = username,
                RecipientUsername = recipient.UserName,
                Content = createMessage.Content,
            };
            messageRepository.AddMessage(massage);
            if (await messageRepository.SaveAllAsync()) return Ok(
                mapper.Map<MessageDto>(massage)
            );

            return BadRequest("Could not send the message u wanker ");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await this.messageRepository.GetMessagesForUser(messageParams);
            Response.AddPagingHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUsername();
            return Ok(
                await this.messageRepository.GetMessageThread(currentUserName, username)
            );
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUserName = User.GetUsername();
            var message = await this.messageRepository.GetMessage(id);
            if (message.Sender.UserName != currentUserName && message.Recipient.UserName != currentUserName)
            {
                return Unauthorized();
            }
            if (message.Sender.UserName == currentUserName) message.SenderDeleted = true;
            if (message.Recipient.UserName == currentUserName) message.RecipientDeleted = true;
            if (message.RecipientDeleted && message.SenderDeleted) this.messageRepository.DeleteMessage(message);
            return await this.messageRepository.SaveAllAsync() ? Ok() : BadRequest(" Problem Deleting Message");

        }

     



    }
}
