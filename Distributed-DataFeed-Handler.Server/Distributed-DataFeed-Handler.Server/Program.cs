var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/debug", () => "SignalR running");

//Web Socket Endpoint
app.MapHub<ChatHub>("/chathub");  // Clients will connect to this URL

app.Run();



