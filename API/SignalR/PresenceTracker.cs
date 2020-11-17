using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string>() { connectionId });
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }
        public Task<bool> UserDisconnect(string username, string connectionId)
        {
            bool isOffline = false;

            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Remove(connectionId);
                    if (OnlineUsers[username].Count == 0)
                    {
                        OnlineUsers.Remove(username);
                        isOffline = true;
                    }
                }

            }
            return Task.FromResult(isOffline);

        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsernames;
            lock (OnlineUsers)
            {
                onlineUsernames = OnlineUsers.Select(x => x.Key).OrderBy(x => x).ToArray();
            }
            return Task.FromResult(onlineUsernames);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock (OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }
            return Task.FromResult(connectionIds);
        }

    }
}
