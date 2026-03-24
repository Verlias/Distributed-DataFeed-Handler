using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<StreamChannelService>();
builder.Services.AddSignalR();
// Need to properly add this
builder.Services.AddHostedService<ProducerBackgroundService>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/debug", () => "SignalR running");

//Web Socket Endpoint
app.MapHub<ChatHub>("/chathub");  // Clients will connect to this URL
app.MapHub<StreamHub>("/streamHub");

app.Run();



