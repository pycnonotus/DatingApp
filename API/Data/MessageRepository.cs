using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public void AddGroup(Group group)
        {
            this.context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string ConnectionId)
        {
            return await this.context.Connections.FindAsync(ConnectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await this.context.Groups.Include(x => x.Connections)
            .Where(x => x.Connections.Any(a => a.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages
            .Include(u => u.Sender)
            .Include(u => u.Recipient)
            .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await this.context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = context.Messages.OrderByDescending(m => m.MessageSent)
            .ProjectTo<MessageDto>(mapper.ConfigurationProvider)
            .AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username
                    && !u.RecipientDeleted
                ),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                    && !u.SenderDeleted
                ),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username && !u.SenderDeleted && u.DateRead == null)
            };

            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);

        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUsername)
        {
            var messages = await this.context.Messages
            .Where(
            m => (m.Recipient.UserName == currentUserName && !m.RecipientDeleted
            && m.Sender.UserName == recipientUsername)
            || (m.Recipient.UserName == recipientUsername
            && m.Sender.UserName == currentUserName && !m.SenderDeleted
            )
            ).OrderBy(x => x.MessageSent)
            .ProjectTo<MessageDto>(mapper.ConfigurationProvider)

            .ToListAsync();



            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientUsername == currentUserName).ToList();
            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.DateRead = DateTime.UtcNow;
                }
                // await this.context.SaveChangesAsync(); //TODO: cheek if the await is need here?
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            this.context.Connections.Remove(connection);
        }


    }
}
