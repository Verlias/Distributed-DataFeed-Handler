using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Distributed_DataFeed_Handler.Application
{
    public class ProducerBackgroundService : BackgroundService
    {
        private readonly StreamChannelService _channelService;
        private readonly ILogger<ProducerBackgroundService> _logger;

        public ProducerBackgroundService(
            StreamChannelService channelService,
            ILogger<ProducerBackgroundService> logger)
        {
            _channelService = channelService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var writer = _channelService.Writer;
            var messages = LoadOrderBooks();
            var processor = new OrderBookReorderProcessor();

            try
            {
                if (messages.Count == 0)
                {
                    _logger.LogWarning("No order book messages were loaded. Producer is idling.");
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                    return;
                }

                foreach (var message in messages)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    foreach (var output in processor.OnMessage(message))
                    {
                        await writer.WriteAsync(output, stoppingToken);
                        _logger.LogInformation("Produced reordered snapshot for seq {Sequence}", message.seq);
                    }

                    await Task.Delay(250, stoppingToken);
                }

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            finally
            {
                writer.TryComplete();
            }
        }

        private List<OrderBook> LoadOrderBooks()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "OrderBooks.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "OrderBooks.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "Distributed-DataFeed-Handler.Application", "OrderBooks.json")
            };

            var filePath = candidates.FirstOrDefault(File.Exists);
            if (filePath is null)
            {
                _logger.LogWarning("Could not find OrderBooks.json. Checked: {Candidates}", string.Join(" | ", candidates));
                return new List<OrderBook>();
            }

            var jsonString = File.ReadAllText(filePath);
            var messages = JsonSerializer.Deserialize<List<OrderBook>>(jsonString) ?? new List<OrderBook>();
            _logger.LogInformation("Loaded {Count} order book messages from {Path}", messages.Count, filePath);

            return messages;
        }
    }

    public class OrderBook
    {
        public int seq { get; set; }
        public string symbol { get; set; } = string.Empty;
        public string timestamp { get; set; } = string.Empty;
        public List<Order> bids { get; set; } = new();
        public List<Order> asks { get; set; } = new();
    }

    public class Order
    {
        public string p { get; set; } = string.Empty;
        public string s { get; set; } = string.Empty;
    }

    public class OrderBookReorderProcessor
    {
        private long expectedSeq = 0;
        private readonly SortedDictionary<long, OrderBook> buffer = new();

        private readonly OrderBook book = new();

        public List<string> OnMessage(OrderBook msg)
        {
            var outputs = new List<string>();

            if (expectedSeq == 0)
            {
                expectedSeq = msg.seq;
            }

            if (msg.seq == expectedSeq)
            {
                outputs.Add(Apply(msg));
                expectedSeq++;

                while (buffer.TryGetValue(expectedSeq, out var next))
                {
                    buffer.Remove(expectedSeq);
                    outputs.Add(Apply(next));
                    expectedSeq++;
                }
            }
            else if (msg.seq > expectedSeq)
            {
                buffer[msg.seq] = msg;
            }

            return outputs;
        }

        private string Apply(OrderBook msg)
        {
            book.seq = msg.seq;
            book.symbol = msg.symbol;
            book.timestamp = msg.timestamp;
            book.bids = msg.bids;
            book.asks = msg.asks;

            return FormatBook();
        }

        private string FormatBook()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"SEQ {book.seq} {book.symbol}");
            builder.AppendLine($"Timestamp: {book.timestamp}");
            builder.AppendLine("ASKS");

            foreach (var a in book.asks)
            {
                builder.AppendLine($"{a.p} {a.s}");
            }

            builder.AppendLine("------");
            builder.AppendLine("BIDS");

            foreach (var b in book.bids)
            {
                builder.AppendLine($"{b.p} {b.s}");
            }

            var snapshot = builder.ToString();
            Console.WriteLine(snapshot);
            return snapshot;
        }
    }
}