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
        private readonly IMessageRepository messageRepository;
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IHubContext<PresenceHub> presenceHub;
        private readonly PresenceTracker presenceTracker;
        public MessageHub(IMessageRepository messageRepository, IMapper mapper, IUserRepository userRepository, IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            this.presenceTracker = presenceTracker;
            this.presenceHub = presenceHub;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.messageRepository = messageRepository;
        }

        public override async Task OnConnectedAsync()
        {
            string username = Context.User.GetUsername();
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = this.GetGroupName(username, otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var group = await AddToGroup(groupName);
            await Clients.Group(groupName).SendAsync("UpdateGroup", group);

            var messages = await messageRepository.GetMessageThread(username, otherUser);
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);


        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string username = Context.User.GetUsername();
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = this.GetGroupName(username, otherUser);
            var group = await RemoveFromMessageGroup();
            await Clients.Group(groupName).SendAsync("UpdateGroup", group);

            await base.OnDisconnectedAsync(exception);
        }
        private string GetGroupName(string caller, string other)
        {
            var stringComper = string.CompareOrdinal(caller, other) < 0;
            return stringComper ? $"{caller}-{other}" : $"{other}-{caller}";
        }

        public async Task sendMessage(CreateMessageDto createMessage)
        {
            var username = Context.User.GetUsername();

            if (username == createMessage.RecipientUsername.ToLower()) throw new HubException("you cant talk with ur self");

            var sender = await userRepository.GetUserByUsernameAsync(username);
            var recipient = await userRepository.GetUserByUsernameAsync(createMessage.RecipientUsername);
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
            var group = await messageRepository.GetMessageGroup(groupName);
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
            messageRepository.AddMessage(massage);
            if (await messageRepository.SaveAllAsync())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", this.mapper.Map<MessageDto>(massage));
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await messageRepository.GetMessageGroup(groupName);
            var connection = new Connection(this.Context.ConnectionId, this.Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await messageRepository.SaveAllAsync()) return group;
            throw new HubException("Fail to join the group");
        }
        private async Task<Group> RemoveFromMessageGroup()
        {
            var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            messageRepository.RemoveConnection(connection);
            if (await messageRepository.SaveAllAsync()) return group;
            throw new HubException("failed to remove from group");
        }

    }
}
