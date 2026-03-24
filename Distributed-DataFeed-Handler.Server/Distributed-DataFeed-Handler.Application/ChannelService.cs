using System.Threading.Channels;

public class StreamChannelService
{
    private readonly Channel<string> _channel;

    public StreamChannelService()
    {
        _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        });
    }

    public ChannelWriter<string> Writer => _channel.Writer;
    public ChannelReader<string> Reader => _channel.Reader;

}