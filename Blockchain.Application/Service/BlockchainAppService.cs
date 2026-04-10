using Blockchain.Application.DTO;
using Blockchain.Domain.Entities;
using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.External;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.Application.Service
{
    public class BlockchainAppService
    {
        private readonly IBlockchainRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IBlockchainService _external;

        public BlockchainAppService(
            IBlockchainRepository repo,
            IUnitOfWork uow,
            IBlockchainService external)
        {
            _repo = repo;
            _uow = uow;
            _external = external;
        }

        public async Task<int> FetchAndStoreAsync(string type)
        {
            var json = await _external.FetchDataAsync(type);

            var entity = new BlockchainData
            {
                BlockchainType = type,
                JsonData = json,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return entity.Id;
        }

        public async Task<List<BlockchainDataDto>> GetHistoryAsync(string type)
        {
            var data = await _repo.GetByTypeAsync(type);

            return data.Select(x => new BlockchainDataDto
            {
                BlockchainType = x.BlockchainType,
                JsonData = x.JsonData,
                CreatedAt = x.CreatedAt
            }).ToList();
        }
    }
}
