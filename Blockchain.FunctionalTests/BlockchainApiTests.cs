using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Blockchain.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Blockchain.Infrastructure.External;
using Blockchain.Infrastructure.Logging;
using Moq;

namespace Blockchain.FunctionalTests
{
    public class BlockchainApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public BlockchainApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the real HttpClient service for IBlockchainService
                    var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IBlockchainService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add mocked IBlockchainService
                    var mockBlockchainService = new Mock<IBlockchainService>();
                    mockBlockchainService
                        .Setup(x => x.FetchDataAsync(It.IsAny<string>()))
                        .ReturnsAsync("{\"network\": \"test\", \"status\": \"ok\"}");

                    services.AddScoped(_ => mockBlockchainService.Object);
                });
            });

            _client = _factory.CreateClient();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }
        }

        [Fact]
        public async Task GetHistory_WithValidBlockchainType_ReturnsOKWithData()
        {
            // Arrange - First store some data
            var response = await _client.PostAsync("/api/blockchain/BTC", new StringContent(""));
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Act
            var getResponse = await _client.GetAsync("/api/blockchain/BTC");

            // Assert
            getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await getResponse.Content.ReadAsStringAsync();
            content.Should().Contain("BTC");
        }

        [Fact]
        public async Task GetHistory_WithInvalidBlockchainType_ReturnsBadRequest()
        {
            // Act
            var response = await _client.GetAsync("/api/blockchain/INVALID");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task FetchData_WithValidBlockchainType_ReturnsOKWithId()
        {
            // Act
            var response = await _client.PostAsync("/api/blockchain/ETH", new StringContent(""));

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("id");
        }

        [Fact]
        public async Task FetchData_WithInvalidBlockchainType_ReturnsBadRequest()
        {
            // Act
            var response = await _client.PostAsync("/api/blockchain/INVALID", new StringContent(""));

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("BTC")]
        [InlineData("ETH")]
        [InlineData("LTC")]
        [InlineData("DASH")]
        public async Task FetchData_WithValidTypes_ReturnsOK(string type)
        {
            // Act
            var response = await _client.PostAsync($"/api/blockchain/{type}", new StringContent(""));

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task HealthCheck_ReturnsOK()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}