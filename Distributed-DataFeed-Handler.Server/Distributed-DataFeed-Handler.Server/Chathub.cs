using System;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    //Web Socket Method
    public async Task SendMessage(string user, string message)
    {
        // Call the "ReceiveMessage" method on all clients connected to the hub
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}