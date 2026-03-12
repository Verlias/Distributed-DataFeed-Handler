#include "alpaca.hpp"

int main() {
	AlpacaClient ap;
	ap.req_quotes(
			true,
			"AAPL");
	return 0;
}
