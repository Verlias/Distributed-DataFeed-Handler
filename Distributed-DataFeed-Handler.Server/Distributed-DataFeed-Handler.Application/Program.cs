using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Threading.Channels;

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

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var message = $"Event at {DateTime.UtcNow:0}";
                    await writer.WriteAsync(message, stoppingToken); // Blocks If channel is full
                    _logger.LogInformation("Produced: {Message}", message);

                    await Task.Delay(500, stoppingToken);
                }
            }
            finally
            {
                writer.Complete(); // Signal no more items
            }
        }
    }

    public class OrderBook
    {
        // Define properties for the OrderBook Json Structure
        public int seq { get; set;}
        public string symbol { get; set; }
        public string timestamp { get; set; }

        // Have to Create a Order Class to accomdate for the structure of the nested Json Array for Bids and Asks
        public List<Order> bids { get; set; }
        public List<Order> asks { get; set; }
    }

    public class Order
    {
        public string p { get; set; }
        public string s { get; set; }
    }

    public class OrderBookReorderProcessor
    {
        private long expectedSeq = 0;
        private SortedDictionary<long, OrderBook> buffer = 
            new SortedDictionary<long, OrderBook>();

        private OrderBook book = new OrderBook();

        public void OnMessage(OrderBook msg)
        {
            if (expectedSeq == 0)
            {
                expectedSeq = msg.seq;
            }

            if (msg.seq == expectedSeq)
            {
                Apply(msg);
                expectedSeq++;

                // Process Buffered Messages
                while (buffer.TryGetValue(expectedSeq, out var next))
                {
                    buffer.Remove(expectedSeq);
                    Apply(next);
                    expectedSeq++;
                }
            }
            else if (msg.seq > expectedSeq)
            {
                //Store Later Messages in Buffer
                buffer[msg.seq] = msg;
            }
            else
            {
                // Old Message -> Ignore
            }
        }

        private void Apply(OrderBook msg)
        {
            book.seq = msg.seq;
            book.symbol = msg.symbol;
            book.timestamp = msg.timestamp;
            book.bids = msg.bids;
            book.asks = msg.asks;
            PrintBook();
        }

        private void PrintBook()
        {
            Console.WriteLine($"SEQ {book.seq} {book.symbol}");
            Console.WriteLine($"Timestamp: {book.timestamp}");

            Console.WriteLine("ASKS");

            foreach (var a in book.asks)
                Console.WriteLine($"{a.p} {a.s}");
            
            Console.WriteLine("------");

            Console.WriteLine("BIDS");

            foreach (var b in book.bids)
                Console.WriteLine($"{b.p} {b.s}");
        }
}

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var processor = new OrderBookReorderProcessor();
            // Path to Json File
            string filePath = "Path";
            string jsonString = File.ReadAllText(filePath);
            
            // List<OrderBook> orderBooks = JsonSerializer.Deserialize<List<OrderBook>>(jsonString);
            var messages = JsonSerializer.Deserialize<List<OrderBook>>(jsonString);
            
            foreach (var msg in messages)
            {
                processor.OnMessage(msg);
            }
            // List of OrderBook 
            // foreach (var orderBook in orderBooks)
            // {
            //     Console.WriteLine($"Timestamp: {orderBook.timestamp} | OrderBook: {orderBook.symbol}");
            // }
        }
    }
}