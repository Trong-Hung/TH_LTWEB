// File: ~/Services/UserConnectionManager.cs
using System.Collections.Concurrent;
using VoTrongHung2280601119.Services;

public class UserConnectionManager : IUserConnectionManager
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> userConnectionMap = new ConcurrentDictionary<string, HashSet<string>>();

    public void AddUserConnection(string userId, string connectionId)
    {
        var userConnections = userConnectionMap.GetOrAdd(userId, _ => new HashSet<string>());
        lock (userConnections)
        {
            userConnections.Add(connectionId);
        }
    }

    public HashSet<string> GetUserConnections(string userId)
    {
        userConnectionMap.TryGetValue(userId, out var connections);
        return connections ?? new HashSet<string>();
    }

    public void RemoveUserConnection(string connectionId)
    {
        // Tìm đúng user chứa connectionId này để xóa
        foreach (var pair in userConnectionMap)
        {
            lock (pair.Value)
            {
                if (pair.Value.Contains(connectionId))
                {
                    pair.Value.Remove(connectionId);
                    // Nếu user không còn kết nối nào thì xóa khỏi danh sách online
                    if (pair.Value.Count == 0)
                    {
                        userConnectionMap.TryRemove(pair.Key, out _);
                    }
                    break;
                }
            }
        }
    }

    public IEnumerable<string> GetOnlineUsers()
    {
        return userConnectionMap.Keys.ToList();
    }
}