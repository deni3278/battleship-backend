using System.Diagnostics;
using BattleshipBackend.Models;
using Microsoft.AspNetCore.SignalR;

namespace BattleshipBackend.Hubs;

public class BattleshipHub : Hub
{
    private static readonly Dictionary<string, User> Users = new();
    private static readonly Dictionary<string, Room> Rooms = new();

    public void SetDisplayName(string displayName)
    {
        Users[Context.ConnectionId].DisplayName = displayName;
        
        Debug.WriteLine(Context.ConnectionId + ": Display name is set to '" + displayName + "'.");
    }

    public IEnumerable<Room> GetRooms()
    {
        return Rooms.Values.ToArray();
    }

    public void LeaveRoom()
    {
        var user = Users[Context.ConnectionId];

        if (user.Room == null) return;

        if (user.Room.Owner == user)
        {
            Rooms.Remove(user.Room.Name);

            if (user.Room.Opponent != null)
            {
                // TODO: Notify the opponent.
                // await Clients.Client(user.Room.Opponent.ConnectionId).SendAsync("LeaveRoom");
            }

            Debug.WriteLine(Context.ConnectionId + ": Removing room with name '" + user.Room.Name + "'.");
        }
        else if (user.Room.Opponent == user)
        {
            user.Room.Opponent = null;
            
            // TODO: Notify the owner.
            // await Clients.Client(user.Room.Owner.ConnectionId).SendAsync("Refresh", user.Room);
            
            Debug.WriteLine(Context.ConnectionId + ": Leaving room with name '" + user.Room.Name + "'.");
        }

        user.Room = null;
    }

    public override Task OnConnectedAsync()
    {
        Debug.WriteLine(Context.ConnectionId + ": Connected.");
        
        Users.Add(Context.ConnectionId, new User(Context.ConnectionId));

        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Debug.WriteLine(Context.ConnectionId + ": Disconnected.");
        
        LeaveRoom();
        
        Users.Remove(Context.ConnectionId);

        return Task.CompletedTask;
    }
}