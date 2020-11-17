using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMapper mapper;
        private readonly IHubContext<PresenceHub> presenceHub;
        private readonly PresenceTracker presenceTracker;
        private readonly IUnitOfWork unitOfWork;
        public MessageHub(IUnitOfWork unitOfWork, IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.presenceTracker = presenceTracker;
            this.presenceHub = presenceHub;
            this.mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            string username = Context.User.GetUsername();
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(username, otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdateGroup", group);

            var messages = await unitOfWork.MessageRepository.GetMessageThread(username, otherUser);

            if(unitOfWork.HasChanges()){
                await unitOfWork.Complete();
            }

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);


        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string username = Context.User.GetUsername();
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(username, otherUser);
            var group = await RemoveFromMessageGroup();
            await Clients.Group(groupName).SendAsync("UpdateGroup", group);

            await base.OnDisconnectedAsync(exception);
        }
        private static string GetGroupName(string caller, string other)
        {
            var stringComper = string.CompareOrdinal(caller, other) < 0;
            return stringComper ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public async Task SendMessage(CreateMessageDto createMessage)
        {
            var username = Context.User.GetUsername();

            if (username == createMessage.RecipientUsername.ToLower()) throw new HubException("you cant talk with ur self");

            var sender = await unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await unitOfWork.UserRepository.GetUserByUsernameAsync(createMessage.RecipientUsername);
            if (recipient == null) throw new HubException("recipient not found");
            var massage = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = username,
                RecipientUsername = recipient.UserName,
                Content = createMessage.Content,
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                massage.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await this.presenceTracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null)
                {
                    await this.presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new
                    {
                        username = sender.UserName,
                        known_as = sender.KnownAs
                    }
                    );
                }
            }
            unitOfWork.MessageRepository.AddMessage(massage);
            if (await unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", this.mapper.Map<MessageDto>(massage));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(this.Context.ConnectionId, this.Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                unitOfWork.MessageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await unitOfWork.Complete()) return group;
            throw new HubException("Fail to join the group");
        }
        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            unitOfWork.MessageRepository.RemoveConnection(connection);
            if (await unitOfWork.Complete()) return group;
            throw new HubException("failed to remove from group");
        }

    }
}
