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

    public Room? CreateRoom(string roomName)
    {
        var exists = Rooms.ContainsKey(roomName);

        if (exists) return null;

        var user = Users[Context.ConnectionId];
        var room = new Room(roomName, user);
        user.Room = room;
        
        Rooms.Add(roomName, room);
        
        Debug.WriteLine(Context.ConnectionId + ": Created a room with name '" + roomName + "'.");

        return room;
    }

    public async Task<Room?> JoinRoom(string roomName)
    {
        if (!Rooms.TryGetValue(roomName, out var room) || room.Opponent != null) return null;
        
        Debug.WriteLine(Context.ConnectionId + ": Joining room with name '" + roomName + "'.");

        var user = Users[Context.ConnectionId];
        user.Room = room;
        room.Opponent = user;

        await Clients.Client(room.Owner.ConnectionId).SendAsync("Refresh", room);
            
        return room;
    }

    public async void LeaveRoom()
    {
        var user = Users[Context.ConnectionId];
        
        if (user.Room == null) return;
        
        Debug.WriteLine(Context.ConnectionId + ": Leaving their room.");
        Debug.WriteLine("Owner: " + user.Room.Owner.DisplayName);
        Debug.WriteLine("Opponent: " + user.Room.Opponent?.DisplayName + "\n");
        
        if (user.Room.Owner == user)
        {
            Rooms.Remove(user.Room.Name);

            if (user.Room.Opponent != null)
            {
                await Clients.Client(user.Room.Opponent.ConnectionId).SendAsync("OwnerLeft");
            }

            Debug.WriteLine(Context.ConnectionId + ": Removing room with name '" + user.Room.Name + "'.");
        }
        else if (user.Room.Opponent == user)
        {
            user.Room.Opponent = null;

            await Clients.Client(user.Room.Owner.ConnectionId).SendAsync("Refresh", user.Room);

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