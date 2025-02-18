using Microsoft.AspNetCore.SignalR;

namespace GameWebServer.Hubs
{
    public class ClientHub : Hub
    {
        public async Task SendMessage(string name, string lobbyCode)
        {
            Console.WriteLine($"{name} joined lobby with code {lobbyCode}");
        }
    }
}
