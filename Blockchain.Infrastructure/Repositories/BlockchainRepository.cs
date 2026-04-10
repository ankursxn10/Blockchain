using Blockchain.Domain.Entities;
using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Infrastructure.Repositories
{
    public class BlockchainRepository : IBlockchainRepository
    {
        private readonly AppDbContext _context;

        public BlockchainRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(BlockchainData data)
        {
            await _context.BlockchainData.AddAsync(data);
        }

        public async Task<List<BlockchainData>> GetByTypeAsync(string type)
        {
            return await _context.BlockchainData
                .Where(x => x.BlockchainType == type)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}
