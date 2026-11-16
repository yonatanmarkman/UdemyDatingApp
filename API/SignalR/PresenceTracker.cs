using System.Collections.Concurrent;

namespace API.SignalR;

public class PresenceTracker
{
    // This dictionary stores all the connections of the online users.
    // Each online user can connect to the website via more than one connection,
    // so therefore, each user gets a concurrent dictionary of connections,
    // and each connection is related to a different device.
    // (Allowing the user to connect to the website via more than one device)
    private static readonly ConcurrentDictionary<string, 
        ConcurrentDictionary<string, byte>> OnlineUsersToConnections = new();
    
    public Task UserConnected(string userId, string connectionId)
    {
        ConcurrentDictionary<string, byte> connections
             = OnlineUsersToConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());

        connections.TryAdd(connectionId, 0);

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string userId, string connectionId)
    {
        ConcurrentDictionary<string, byte>? connections;
        if (OnlineUsersToConnections.TryGetValue(userId, out connections))
        {
            connections.TryRemove(connectionId, out _);
            if (connections.IsEmpty)
            {
                OnlineUsersToConnections.TryRemove(userId, out _);
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        return Task.FromResult(OnlineUsersToConnections.Keys
                                .OrderBy(k => k)
                                .ToArray());
    }

    public static Task<List<string>> GetConnectionsForUser(string userId)
    {
        if (OnlineUsersToConnections.TryGetValue(userId, out ConcurrentDictionary<string, byte>? connections))
        {
            return Task.FromResult(connections.Keys.ToList());
        }

        return Task.FromResult(new List<string>());
    }
}
