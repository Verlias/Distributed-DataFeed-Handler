cmake_minimum_required(VERSION 4.1.2)

project(AlpacaTest)

find_package(Boost REQUIRED)
find_package(OpenSSL REQUIRED)

add_executable(alpaca alpaca.cpp)

target_link_directories(alpaca PRIVATE
	${Boost_INCLUDE_DIRS}
)

target_link_libraries(alpaca
	OpenSSL::SSL
	OpenSSL::Crypto
)
