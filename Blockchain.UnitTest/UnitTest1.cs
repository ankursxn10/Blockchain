using Blockchain.Application.Service;
using Blockchain.Application.Validators;
using Blockchain.Domain.Entities;
using Blockchain.Domain.Interfaces;
using Blockchain.Infrastructure.Caching;
using Blockchain.Infrastructure.External;
using Blockchain.Infrastructure.Logging;
using Moq;

namespace Blockchain.UnitTest
{
    public class BlockchainAppServiceTests
    {
        private readonly Mock<IBlockchainRepository> _mockRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IBlockchainService> _mockExternalService;
        private readonly Mock<ICacheService> _mockCache;
        private readonly Mock<IBlockchainLogger> _mockLogger;
        private readonly BlockchainAppService _service;

        public BlockchainAppServiceTests()
        {
            _mockRepository = new Mock<IBlockchainRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockExternalService = new Mock<IBlockchainService>();
            _mockCache = new Mock<ICacheService>();
            _mockLogger = new Mock<IBlockchainLogger>();

            _service = new BlockchainAppService(
                _mockRepository.Object,
                _mockUnitOfWork.Object,
                _mockExternalService.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task FetchAndStoreAsync_WithValidType_ReturnsEntityId()
        {
            // Arrange
            var type = "BTC";
            var jsonData = "{\"network\": \"btc\"}";
            _mockExternalService.Setup(x => x.FetchDataAsync(type))
                .ReturnsAsync(jsonData);

            // Mock the AddAsync method to set an ID on the entity
            _mockRepository.Setup(x => x.AddAsync(It.IsAny<BlockchainData>()))
                .Callback<BlockchainData>(d => d.Id = 1)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.FetchAndStoreAsync(type);

            // Assert
            Assert.True(result > 0);
            _mockRepository.Verify(x => x.AddAsync(It.IsAny<BlockchainData>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task FetchAndStoreAsync_WithInvalidType_ThrowsArgumentException()
        {
            // Arrange
            var type = "INVALID";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.FetchAndStoreAsync(type));
        }

        [Fact]
        public async Task FetchAndStoreAsync_WithEmptyType_ThrowsArgumentException()
        {
            // Arrange
            var type = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.FetchAndStoreAsync(type));
        }

        [Fact]
        public async Task GetHistoryAsync_WithCachedData_ReturnsCachedResult()
        {
            // Arrange
            var type = "BTC";
            var cachedData = new List<Blockchain.Application.DTO.BlockchainDataDto>
            {
                new() { Id = 1, BlockchainType = type, JsonData = "{}", CreatedAt = DateTime.UtcNow }
            };
            _mockCache.Setup(x => x.GetAsync<List<Blockchain.Application.DTO.BlockchainDataDto>>(It.IsAny<string>()))
                .ReturnsAsync(cachedData);

            // Act
            var result = await _service.GetHistoryAsync(type);

            // Assert
            Assert.Equal(cachedData, result);
            _mockRepository.Verify(x => x.GetByTypeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetHistoryAsync_WithInvalidType_ThrowsArgumentException()
        {
            // Arrange
            var type = "INVALID";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetHistoryAsync(type));
        }

        [Theory]
        [InlineData("BTC")]
        [InlineData("ETH")]
        [InlineData("LTC")]
        [InlineData("DASH")]
        public void BlockchainRequestValidator_WithValidType_ReturnsTrue(string type)
        {
            // Act
            var result = BlockchainRequestValidator.IsValidBlockchainType(type);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("INVALID")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void BlockchainRequestValidator_WithInvalidType_ReturnsFalse(string? type)
        {
            // Act
            var result = BlockchainRequestValidator.IsValidBlockchainType(type ?? "");

            // Assert
            Assert.False(result);
        }
    }

    public class BlockchainRequestValidatorTests
    {
        [Theory]
        [InlineData("BTC")]
        [InlineData("ETH")]
        [InlineData("LTC")]
        [InlineData("DASH")]
        public void GetValidationError_WithValidType_ReturnsEmptyString(string type)
        {
            // Act
            var result = BlockchainRequestValidator.GetValidationError(type);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("INVALID")]
        public void GetValidationError_WithInvalidType_ReturnsErrorMessage(string type)
        {
            // Act
            var result = BlockchainRequestValidator.GetValidationError(type);

            // Assert
            Assert.NotEmpty(result);
        }
    }

    public class CacheServiceTests
    {
        [Fact]
        public async Task SetAsync_AndGetAsync_ReturnsStoredValue()
        {
            // Arrange
            var service = new Blockchain.Infrastructure.Caching.InMemoryCacheService();
            var key = "test_key";
            var value = "test_value";

            // Act
            await service.SetAsync(key, value);
            var result = await service.GetAsync<string>(key);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task GetAsync_WithNonExistentKey_ReturnsNull()
        {
            // Arrange
            var service = new Blockchain.Infrastructure.Caching.InMemoryCacheService();

            // Act
            var result = await service.GetAsync<string>("non_existent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveAsync_RemovesValueFromCache()
        {
            // Arrange
            var service = new Blockchain.Infrastructure.Caching.InMemoryCacheService();
            var key = "test_key";
            await service.SetAsync(key, "value");

            // Act
            await service.RemoveAsync(key);
            var result = await service.GetAsync<string>(key);

            // Assert
            Assert.Null(result);
        }
    }
}
