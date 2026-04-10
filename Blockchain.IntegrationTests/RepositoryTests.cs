using Blockchain.Domain.Entities;
using Blockchain.Infrastructure.Persistence;
using Blockchain.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockchain.IntegrationTests
{
    public class RepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_Should_Save_Data()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new BlockchainRepository(context);

            var data = new BlockchainData
            {
                BlockchainType = "BTC",
                JsonData = "test",
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repo.AddAsync(data);
            await context.SaveChangesAsync();

            // Assert
            context.BlockchainData.Count().Should().Be(1);
        }

        [Fact]
        public async Task GetByTypeAsync_Should_Return_Ordered_Data()
        {
            // Arrange
            var context = GetDbContext();
            var repo = new BlockchainRepository(context);

            context.BlockchainData.AddRange(
                new BlockchainData { BlockchainType = "BTC", JsonData = "test1", CreatedAt = DateTime.UtcNow },
                new BlockchainData { BlockchainType = "BTC", JsonData = "test2", CreatedAt = DateTime.UtcNow.AddMinutes(-1) }
            );

            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByTypeAsync("BTC");

            // Assert
            result.First().CreatedAt.Should().BeAfter(result.Last().CreatedAt);
        }
    }
}