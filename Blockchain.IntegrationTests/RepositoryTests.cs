using Blockchain.Domain.Entities;
using Blockchain.Infrastructure.Logging;
using Blockchain.Infrastructure.Persistence;
using Blockchain.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;

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

        private Mock<IBlockchainLogger> GetMockLogger()
        {
            return new Mock<IBlockchainLogger>();
        }

        [Fact]
        public async Task AddAsync_Should_Save_Data()
        {
            // Arrange
            var context = GetDbContext();
            var mockLogger = GetMockLogger();
            var repo = new BlockchainRepository(context, mockLogger.Object);

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
            var mockLogger = GetMockLogger();
            var repo = new BlockchainRepository(context, mockLogger.Object);

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

        [Fact]
        public async Task GetByTypeAsync_Should_Return_Empty_When_Type_Not_Found()
        {
            // Arrange
            var context = GetDbContext();
            var mockLogger = GetMockLogger();
            var repo = new BlockchainRepository(context, mockLogger.Object);

            context.BlockchainData.Add(
                new BlockchainData { BlockchainType = "BTC", JsonData = "test", CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByTypeAsync("ETH");

            // Assert
            result.Should().BeEmpty();
        }
    }

    public class UnitOfWorkTests
    {
        [Fact]
        public async Task SaveChangesAsync_Should_Save_Changes()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            var mockLogger = new Mock<IBlockchainLogger>();
            var uow = new UnitOfWork(context, mockLogger.Object);

            context.BlockchainData.Add(new BlockchainData
            {
                BlockchainType = "BTC",
                JsonData = "test",
                CreatedAt = DateTime.UtcNow
            });

            // Act
            await uow.SaveChangesAsync();

            // Assert
            var saved = context.BlockchainData.Count();
            saved.Should().Be(1);
        }
    }
}