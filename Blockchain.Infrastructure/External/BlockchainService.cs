using Blockchain.Infrastructure.Logging;

namespace Blockchain.Infrastructure.External
{
    public class BlockchainService : IBlockchainService
    {
        private readonly HttpClient _httpClient;
        private readonly IBlockchainLogger _logger;

        private static readonly Dictionary<string, string> BlockchainUrls = new()
        {
            { "BTC", "https://api.blockcypher.com/v1/btc/main" },
            { "ETH", "https://api.blockcypher.com/v1/eth/main" },
            { "LTC", "https://api.blockcypher.com/v1/ltc/main" },
            { "DASH", "https://api.blockcypher.com/v1/dash/main" }
        };

        public BlockchainService(HttpClient httpClient, IBlockchainLogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> FetchDataAsync(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                _logger.LogWarning("Fetch request with empty blockchain type");
                throw new ArgumentException("Blockchain type cannot be null or empty.", nameof(type));
            }

            var upperType = type.ToUpperInvariant();

            if (!BlockchainUrls.TryGetValue(upperType, out var url))
            {
                _logger.LogWarning("Invalid blockchain type requested: {BlockchainType}", type);
                throw new ArgumentException($"Invalid blockchain type: {type}. Valid types are: {string.Join(", ", BlockchainUrls.Keys)}", nameof(type));
            }

            try
            {
                _logger.LogInfo("Fetching data for blockchain type: {BlockchainType} from {Url}", type, url);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var response = await _httpClient.GetStringAsync(url, cts.Token);

                _logger.LogInfo("Successfully fetched data for blockchain type: {BlockchainType}", type);
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request failed for blockchain type: {{BlockchainType}}", ex, type);
                throw new InvalidOperationException($"Failed to fetch data from BlockCypher for {type}.", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"Request timeout for blockchain type: {{BlockchainType}}", ex, type);
                throw new TimeoutException($"Request timed out while fetching data for {type}.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching data for blockchain type: {{BlockchainType}}", ex, type);
                throw;
            }
        }
    }
}
