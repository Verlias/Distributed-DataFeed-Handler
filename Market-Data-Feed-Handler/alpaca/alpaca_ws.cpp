#include "alpaca_ws.hpp"

int main() {
	AlpacaClient ap;
	ap.req_quotes(false, "AAPL");
	return 0;
}
