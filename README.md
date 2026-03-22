# Distributed-DataFeed-Handler

Low-latency distributed market data feed handler built with C++ and C#.  
Consumes Alpaca market data, generates order book snapshots, streams them over a custom TCP binary protocol, applies sequence-based reordering in a C# feed processor, and visualizes the live order book on a frontend client.
