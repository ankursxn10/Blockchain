using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Infrastructure.External
{
    public class BlockchainService : IBlockchainService
    {
        private readonly HttpClient _httpClient;

        public BlockchainService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> FetchDataAsync(string type)
        {
            var url = type switch
            {
                "BTC" => "https://api.blockcypher.com/v1/btc/main",
                "ETH" => "https://api.blockcypher.com/v1/eth/main",
                "LTC" => "https://api.blockcypher.com/v1/ltc/main",
                "DASH" => "https://api.blockcypher.com/v1/dash/main",
                _ => throw new Exception("Invalid type")
            };

            return await _httpClient.GetStringAsync(url);
        }
    }
}
