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

    public async Task LeaveRoom()
    {
        var user = Users[Context.ConnectionId];
        var room = user.Room;
        
        if (room == null) return;
        
        Debug.WriteLine(Context.ConnectionId + ": Leaving their room.");
        Debug.WriteLine("Owner: " + room.Owner.DisplayName);
        Debug.WriteLine("Opponent: " + room.Opponent?.DisplayName + "\n");
        
        Unready();
        
        if (room.Owner == user)
        {
            Rooms.Remove(room.Name);

            if (room.Opponent != null)
            {
                room.IsOwnerReady = false;
                
                await Clients.Client(room.Opponent.ConnectionId).SendAsync("OwnerLeft");
            }

            Debug.WriteLine(Context.ConnectionId + ": Removing room with name '" + room.Name + "'.");
        }
        else if (room.Opponent == user)
        {
            room.Opponent = null;
            room.IsOpponentReady = false;

            await Clients.Client(room.Owner.ConnectionId).SendAsync("Refresh", room);

            Debug.WriteLine(Context.ConnectionId + ": Leaving room with name '" + room.Name + "'.");
        }
        else
        {
            Debug.WriteLine(Context.ConnectionId + ": Something unexpected happened while attempting to leave room.");
        }
        
        user.Room = null;
    }

    public async Task Ready()
    {
        var user = Users[Context.ConnectionId];
        var room = user.Room;

        if (room == null) return;
        
        Debug.WriteLine(Context.ConnectionId + ": Readying in room with name '" + room.Name + "'.");
        
        var isOwner = user.Room.Owner == user;

        if (isOwner)
        {
            room.IsOwnerReady = true;
        }
        else
        {
            room.IsOpponentReady = true;
        }

        if (room.IsOwnerReady && room.IsOpponentReady)
        {
            Debug.WriteLine(Context.ConnectionId + ": Game starting in room with name '" + room.Name + "'.");

            await Clients.Client(room.Owner.ConnectionId).SendAsync("Start");
            await Clients.Client(room.Opponent.ConnectionId).SendAsync("Start");
        }
    }

    public void Unready()
    {
        var user = Users[Context.ConnectionId];
        var room = user.Room;
        
        if (room == null) return;
        
        Debug.WriteLine(Context.ConnectionId + ": Unreadying in room with name '" + room.Name + "'.");

        if (room.Owner == user)
        {
            room.IsOwnerReady = false;
        }
        else
        {
            room.IsOpponentReady = false;
        }
    }

    public override Task OnConnectedAsync()
    {
        Debug.WriteLine(Context.ConnectionId + ": Connected.");

        Users.Add(Context.ConnectionId, new User(Context.ConnectionId));
        
        return Task.CompletedTask;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Debug.WriteLine(Context.ConnectionId + ": Disconnected.");

        await LeaveRoom();

        Users.Remove(Context.ConnectionId);

    }
}