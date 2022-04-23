using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

namespace BattleshipBackend.Hubs;

public class BattleshipHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Debug.WriteLine("\n" + Context.ConnectionId + ": Connected to " + nameof(BattleshipHub) + ".");
        
        return base.OnConnectedAsync();
    }
}