using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace Distributed_DataFeed_Handler.Application
{

    public class OrderBook
    {
        // Define properties for the OrderBook Json Structure
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

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // Path to Json File
            string filePath = "Path";
            string jsonString = File.ReadAllText(filePath);
            
            List<OrderBook> orderBooks = JsonSerializer.Deserialize<List<OrderBook>>(jsonString);
           
            // List of OrderBook 
            foreach (var orderBook in orderBooks)
            {
                Console.WriteLine($"Timestamp: {orderBook.timestamp} | OrderBook: {orderBook.symbol}");
            }
        }
    }
}