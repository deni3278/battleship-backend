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

    public Room[] GetRooms()
    {
        return Rooms.Values.ToArray();
    }

    public Room? CreateRoom(string roomName)
    {
        Debug.WriteLine(Context.ConnectionId + ": Attempting to create a room.");
        
        if (Rooms.ContainsKey(roomName)) return null;
        
        var user = Users[Context.ConnectionId];
        var room = new Room(roomName, user);
        user.Room = room;

        Rooms.Add(roomName, room);
        
        Debug.WriteLine(Context.ConnectionId + ": Created room with name '" + room.Name + "'.");

        return room;
    }

    public void LeaveRoom()
    {
        var user = Users[Context.ConnectionId];
        var room = user.Room;

        if (room == null) return;
        
        // TODO: Inform the other user.

        if (room.Owner == user)
        {
            Rooms.Remove(room.Name);
            
            Debug.WriteLine(Context.ConnectionId + ": Delisting room with name '" + room.Name + "'.");
        }
        else if (room.Opponent == user)
        {
            room.Opponent = null;
            
            Debug.WriteLine(Context.ConnectionId + ": Leaving room with name '" + room.Name + "'.");
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