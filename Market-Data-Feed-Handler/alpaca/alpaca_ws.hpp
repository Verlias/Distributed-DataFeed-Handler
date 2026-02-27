#include <boost/beast/core.hpp>
#include <boost/beast/websocket.hpp>
#include <boost/beast/websocket/ssl.hpp>
#include <boost/asio/ssl.hpp>
#include <boost/asio/strand.hpp>
#include <cstdlib>
#include <memory>
#include <iostream>
#include <string>

namespace beast = boost::beast;
namespace http = beast::http;
namespace net = boost::asio;
namespace ssl = boost::asio::ssl;
using tcp = boost::asio::ip::tcp;

#ifndef ALPACA_WS_H
#define ALPACA_WS_H

/**
 * Namespace used for alpaca targets + custom alpaca client
 * */
namespace alpaca {
	const std::string test_stream(std::getenv("WS_TEST_STREAM"));
	const std::string sandbox_env(std::getenv("SANDBOX");	
	const std::string SIP = "/v2/sip";
	const std::string IEX = "/v2/iex";
	const std::string D_SIP = "/v2/delayed_sip";
	const std::string BOATS = "/v1beta1/boats";
	const std::string OVERNIGHT = "/v1beta1/overnight";
}

#endif
