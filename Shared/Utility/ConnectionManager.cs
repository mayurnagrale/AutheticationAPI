using System.Collections.Concurrent;

namespace AutheticationAPI.Shared.Utility
{
    public class ConnectionManager
    {
        private readonly ConcurrentDictionary<string, string> _connections = new();

        public bool AddConnection(string username, string connectionId)
        {
            if (string.IsNullOrEmpty(username)) return false;

            _connections.AddOrUpdate(username, connectionId, (key, oldValue) => connectionId);
            return true;
        }

        public bool RemoveConnection(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;

            return _connections.TryRemove(username, out _);
        }

        public string? GetConnection(string username)
        {
            _connections.TryGetValue(username, out var connectionId);
            return connectionId;
        }

        public List<string> GetAllOnlineUsers()
        {
            return _connections.Keys.ToList();
        }
    }
}
