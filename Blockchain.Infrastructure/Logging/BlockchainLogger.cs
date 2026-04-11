using Microsoft.Extensions.Logging;

namespace Blockchain.Infrastructure.Logging
{
    public class BlockchainLogger : IBlockchainLogger
    {
        private readonly ILogger<BlockchainLogger> _logger;

        public BlockchainLogger(ILogger<BlockchainLogger> logger)
        {
            _logger = logger;
        }

        public void LogInfo(string message, params object?[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object?[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(string message, Exception? ex = null, params object?[] args)
        {
            if (ex != null)
                _logger.LogError(ex, message, args);
            else
                _logger.LogError(message, args);
        }

        public void LogDebug(string message, params object?[] args)
        {
            _logger.LogDebug(message, args);
        }
    }
}
