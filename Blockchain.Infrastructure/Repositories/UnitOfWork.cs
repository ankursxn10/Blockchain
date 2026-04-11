using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.Logging;
using Blockchain.Infrastructure.Persistence;

namespace Blockchain.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IBlockchainLogger _logger;

        public UnitOfWork(AppDbContext context, IBlockchainLogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                _logger.LogDebug("Saving changes to database");
                var changes = await _context.SaveChangesAsync();
                _logger.LogInfo("Database save completed. Changes: {ChangeCount}", changes);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error saving changes to database", ex);
                throw;
            }
        }
    }
}
