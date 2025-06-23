namespace VoTrongHung2280601119.Services
{
    public interface IUserConnectionManager
    {
        void AddUserConnection(string userId, string connectionId);
        void RemoveUserConnection(string connectionId);
        HashSet<string> GetUserConnections(string userId);
        IEnumerable<string> GetOnlineUsers();
    }
}
