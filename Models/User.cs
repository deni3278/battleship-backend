namespace BattleshipBackend.Models;

public class User
{
    public string ConnectionId { get; }
    public string DisplayName { get; set; }
    public Room? Room { get; set; }

    public User(string connectionId)
    {
        ConnectionId = connectionId;
    }
}