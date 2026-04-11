using Blockchain.Application.DTO;
using Blockchain.Application.Validators;
using Blockchain.Domain.Entities;
using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.Caching;
using Blockchain.Infrastructure.External;
using Blockchain.Infrastructure.Logging;

namespace Blockchain.Application.Service
{
    public class BlockchainAppService
    {
        private readonly IBlockchainRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IBlockchainService _external;
        private readonly ICacheService _cache;
        private readonly IBlockchainLogger _logger;
        private const string HistoryCacheKeyPrefix = "blockchain_history_";
        private const int CacheExpirationMinutes = 30;

        public BlockchainAppService(
            IBlockchainRepository repo,
            IUnitOfWork uow,
            IBlockchainService external,
            ICacheService cache,
            IBlockchainLogger logger)
        {
            _repo = repo;
            _uow = uow;
            _external = external;
            _cache = cache;
            _logger = logger;
        }

        public async Task<int> FetchAndStoreAsync(string type)
        {
            _logger.LogInfo("Starting fetch and store operation for blockchain type: {BlockchainType}", type);

            // Validation
            var validationError = BlockchainRequestValidator.GetValidationError(type);
            if (!string.IsNullOrEmpty(validationError))
            {
                _logger.LogWarning("Validation failed: {ValidationError}", validationError);
                throw new ArgumentException(validationError);
            }

            try
            {
                var json = await _external.FetchDataAsync(type);

                var entity = new BlockchainData
                {
                    BlockchainType = type.ToUpperInvariant(),
                    JsonData = json,
                    CreatedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(entity);
                await _uow.SaveChangesAsync();

                // Invalidate cache
                await _cache.RemoveByPatternAsync($"{HistoryCacheKeyPrefix}{type.ToUpperInvariant()}");

                _logger.LogInfo("Successfully fetched and stored data for blockchain type: {BlockchainType}, Entity ID: {EntityId}", type, entity.Id);

                return entity.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during fetch and store operation for blockchain type: {BlockchainType}", ex, type);
                throw;
            }
        }

        public async Task<List<BlockchainDataDto>> GetHistoryAsync(string type)
        {
            _logger.LogInfo("Starting get history operation for blockchain type: {BlockchainType}", type);

            // Validation
            var validationError = BlockchainRequestValidator.GetValidationError(type);
            if (!string.IsNullOrEmpty(validationError))
            {
                _logger.LogWarning("Validation failed: {ValidationError}", validationError);
                throw new ArgumentException(validationError);
            }

            var upperType = type.ToUpperInvariant();
            var cacheKey = $"{HistoryCacheKeyPrefix}{upperType}";

            try
            {
                // Check cache first
                var cachedData = await _cache.GetAsync<List<BlockchainDataDto>>(cacheKey);
                if (cachedData != null)
                {
                    _logger.LogDebug("Retrieved blockchain history from cache for type: {BlockchainType}", upperType);
                    return cachedData;
                }

                // Fetch from database
                var data = await _repo.GetByTypeAsync(upperType);

                var result = data.Select(x => new BlockchainDataDto
                {
                    Id = x.Id,
                    BlockchainType = x.BlockchainType,
                    JsonData = x.JsonData,
                    CreatedAt = x.CreatedAt
                }).ToList();

                // Cache the result
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));

                _logger.LogInfo("Retrieved blockchain history for type: {BlockchainType}, Records: {RecordCount}", upperType, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during get history operation for blockchain type: {BlockchainType}", ex, type);
                throw;
            }
        }
    }
}
