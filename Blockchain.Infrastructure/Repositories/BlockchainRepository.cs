using Blockchain.Domain.Entities;
using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.Logging;
using Blockchain.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Blockchain.Infrastructure.Repositories
{
    public class BlockchainRepository : IBlockchainRepository
    {
        private readonly AppDbContext _context;
        private readonly IBlockchainLogger _logger;

        public BlockchainRepository(AppDbContext context, IBlockchainLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(BlockchainData data)
        {
            try
            {
                _logger.LogDebug("Adding blockchain data for type: {BlockchainType}", data.BlockchainType);
                await _context.BlockchainData.AddAsync(data);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding blockchain data for type: {BlockchainType}", ex, data.BlockchainType);
                throw;
            }
        }

        public async Task<List<BlockchainData>> GetByTypeAsync(string type)
        {
            try
            {
                _logger.LogDebug("Retrieving blockchain data for type: {BlockchainType}", type);
                var result = await _context.BlockchainData
                    .Where(x => x.BlockchainType == type)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {RecordCount} records for blockchain type: {BlockchainType}", result.Count, type);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving blockchain data for type: {BlockchainType}", ex, type);
                throw;
            }
        }
    }
}
