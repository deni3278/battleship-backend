using BattleshipBackend.Hubs;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddCors();
services.AddSignalR();

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    policyBuilder.AllowAnyHeader();
    policyBuilder.AllowAnyMethod();
    policyBuilder.SetIsOriginAllowed(_ => true);
});

app.MapHub<BattleshipHub>("/hubs/battleship");

app.Run();