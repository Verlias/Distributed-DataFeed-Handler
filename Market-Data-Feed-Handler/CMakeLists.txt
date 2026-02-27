cmake_minimum_required(VERSION 4.2.1)

project(MarketDataFeedHandler)

# find packages needed
find_package(nlohmann_json REQUIRED)
find_package(websocketpp REQUIRED)
find_package(OpenSSL REQUIRED)

# Alpaca request executable
add_executable(alpaca alpaca/alpaca.cpp)

target_link_libraries(alpaca 
	websocketpp 
	nlohmann_json::nlohmann_json
	OpenSSL::SSL
	OpenSSL::CRYPTO
)
