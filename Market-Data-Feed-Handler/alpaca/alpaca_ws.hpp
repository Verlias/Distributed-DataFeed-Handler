#include <boost/beast/core.hpp>
#include <boost/beast/http.hpp>
#include <boost/beast/version.hpp>
#include <boost/asio/connect.hpp>
#include <boost/asio/ip/tcp.hpp>
#include <boost/asio/ssl.hpp>
#include <boost/beast/ssl.hpp>
#include <openssl/ssl.h>
#include <openssl/err.h>
#include <cstdlib>
#include <iostream>
#include <string>
#include <ctime>

namespace beast = boost::beast;     // from <boost/beast.hpp>
namespace http = beast::http;       // from <boost/beast/http.hpp>
namespace net = boost::asio;        // from <boost/asio.hpp>
namespace ssl = net::ssl;
using tcp = net::ip::tcp;           // from <boost/asio/ip/tcp.hpp>
							
#ifndef ALPACA_WS_H
#define ALPACA_WS_H

#define HOST "data.alpaca.markets"
#define PORT "443"

class AlpacaClient {
public:
	AlpacaClient() {
		api_key = std::getenv("API_KEY");
		api_sec = std::getenv("SECRET");	
	}
	
	void req_quotes(
			bool hist,
			std::string const &ticker) {
		net::io_context ioc;
		ssl::context ctx(ssl::context::tls_client);
		ctx.set_default_verify_paths();

		tcp::resolver resolver(ioc);
		beast::ssl_stream<beast::tcp_stream> stream(ioc, ctx);
	
		ctx.set_default_verify_paths();

		stream.set_verify_mode(ssl::verify_peer);

		if (!SSL_set_tlsext_host_name(stream.native_handle(), HOST)) {
			beast::error_code ec{
				static_cast<int>(::ERR_get_error()),
				net::error::get_ssl_category()
			};
			throw beast::system_error{ec};
		}

		auto const results = resolver.resolve(HOST, PORT);
		beast::get_lowest_layer(stream).connect(results);
		stream.handshake(ssl::stream_base::client);

		auto verb = http::verb::get;
		std::string uri = get_quotes_uri(hist, ticker);
		http::request<http::string_body> req(verb, uri, 11);

		req.set(http::field::user_agent, BOOST_BEAST_VERSION_STRING);
		req.set(http::field::host, HOST);
		req.set(http::field::content_type, "application/json");
		req.set("APCA-API-KEY-ID", api_key);
		req.set("APCA-API-SECRET-KEY", api_sec);

		// establish connection
		http::write(stream, req);

		beast::flat_buffer buff;

		http::response<http::dynamic_body> res;
		http::read(stream, buff, res);

		std::cout << res << std::endl;
	
		beast::error_code ec;
		stream.shutdown(ec);
		
		if (ec == net::error::eof || ec == ssl::error::stream_truncated) {
			ec = {};
		}

		if (ec) {
			throw beast::system_error{ec};
		}
	}

	std::string get_quotes_uri(
			bool hist,
			std::string const &ticker) {
		std::string uri;
		if (hist) {
			uri = cons_hist_quotes_uri(ticker);
		} else {
			uri = cons_latest_quotes_uri(ticker);
		}

		return uri;
	} 

	std::string cons_hist_quotes_uri(
			std::string const &ticker,
			int limit=1000,
			bool start=false) {
		if (limit < 1 && 10000 < limit) {
			std::cerr << "Construction of historical quotes URI error: limit=" << limit;
			return "";
		}
		
		std::string uri = "/v2/stocks/quotes";
		uri += "?symbols=" + ticker + "&limit=" + std::to_string(limit);
		
		if (start) {
			std::time_t now = std::time(nullptr);
			std::tm* tm = std::gmtime(&now);
			
			tm->tm_mon -= 1;
			mktime(tm);

			char buf[21];
			std::strftime(buf, sizeof(buf), "%Y-%m-%dT%H:%M:%SZ", tm);

			// URL-encode the colons
			std::string time = buf;
			size_t pos = 0;
			while ((pos = time.find(':', pos)) != std::string::npos) {
				time.replace(pos, 1, "%3A");
				pos += 3;
			}

			uri += "&start=" + time;
		}

		return uri;
	}
		
	std::string cons_latest_quotes_uri(
			std::string const &ticker) {
		std::string uri = "/v2/stocks/quotes/latest";
		uri += "?symbols=" + ticker;

		return uri;
	}

private:
	std::string api_key;
	std::string api_sec;
}; 

#endif
