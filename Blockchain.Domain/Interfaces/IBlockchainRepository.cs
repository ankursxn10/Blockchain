using Blockchain.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Domain.Interfaces
{
    public interface IBlockchainRepository
    {
        Task AddAsync(BlockchainData data);
        Task<List<BlockchainData>> GetByTypeAsync(string type);
    }
}
