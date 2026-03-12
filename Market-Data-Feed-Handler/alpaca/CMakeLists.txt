cmake_minimum_required(VERSION 3.20)

project(AlpacaTest)

find_package(Boost REQUIRED COMPONENTS json)
find_package(OpenSSL REQUIRED)

add_executable(alpaca alpaca.cpp)

target_include_directories(alpaca PRIVATE
    ${Boost_INCLUDE_DIRS}
)

target_link_libraries(alpaca
    Boost::json
    OpenSSL::SSL
    OpenSSL::Crypto
)
