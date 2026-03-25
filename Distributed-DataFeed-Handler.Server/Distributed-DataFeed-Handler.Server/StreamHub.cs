using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Distributed_DataFeed_Handler.Application;

public class StreamHub : Hub
{
    private readonly StreamChannelService _channelService;

    public StreamHub(StreamChannelService channelService)
    {
        _channelService = channelService;
    }

    // Client calls this to start recieving the stream
    public async IAsyncEnumerable<string> StreamMessages(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var message in _channelService.Reader.ReadAllAsync(cancellationToken))
        {
            yield return message;
        }
    }

    // Alternative: push model - hub reads and broadcasts to all clients
    public async Task StartBroadcast(CancellationToken cancellationToken)
    {
        await foreach (var message in _channelService.Reader.ReadAllAsync(cancellationToken))
        {
            await Clients.All.SendAsync("RecieveMessage", message, cancellationToken);
        }
    }
}