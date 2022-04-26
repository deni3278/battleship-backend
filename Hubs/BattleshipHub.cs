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
        Debug.WriteLine(Context.ConnectionId + ": Display name is set to '" + displayName + "'.");
        
        Users[Context.ConnectionId].DisplayName = displayName;
    }

    public Room[] GetRooms()
    {
        return Rooms.Values.ToArray();
    }

    public bool CreateRoom(string roomName)
    {
        if (Rooms.ContainsKey(roomName)) return false;

        var user = Users[Context.ConnectionId];
        var room = new Room(roomName, user);
        user.Room = room;
        
        Rooms.Add(roomName, room);

        return true;
    }

    public override Task OnConnectedAsync()
    {
        Debug.WriteLine(Context.ConnectionId + ": Connected.");
        
        Users.Add(Context.ConnectionId, new User(Context.ConnectionId));
        
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Debug.WriteLine(Context.ConnectionId + ": Disconnected.");
        
        // TODO: If the user is in a room with an opponent, the opponent should be notified of the user's disconnection.

        Users.Remove(Context.ConnectionId);
        
        return base.OnDisconnectedAsync(exception);
    }
}