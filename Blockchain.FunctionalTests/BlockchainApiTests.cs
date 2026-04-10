using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Blockchain.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Blockchain.Infrastructure.External;
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
                        .ReturnsAsync("{\"status\": \"test\"}");

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
                dbContext.Database.Migrate();
            }
        }

        [Fact]
        public async Task Get_Should_Return_OK()
        {
            // Act
            var response = await _client.GetAsync("/api/blockchain/BTC");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_Should_Store_Data()
        {
            // Act
            var content = new StringContent("");
            var response = await _client.PostAsync("/api/blockchain/BTC", content);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}