using Infrastructure.Services.Caching.Redis;
using StackExchange.Redis;
using System.Text.Json;
using Testcontainers.Redis;
using Xunit;

namespace Test.Unit.Infrastructure.Caching.Redis
{
    public class RedisServiceTests : IAsyncLifetime
    {
        private readonly RedisContainer redisContainer =
            new RedisBuilder()
                .WithImage("redis/redis-stack-server:7.2.0-v19")
                .Build();

        private IConnectionMultiplexer connectionMultiplexer;
        private RedisService redisService;

        public async Task InitializeAsync()
        {
            await redisContainer.StartAsync();
            connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(redisContainer.GetConnectionString());
            redisService = new RedisService(connectionMultiplexer);
        }

        public async Task DisposeAsync()
        {
            if (connectionMultiplexer != null)
            {
                await connectionMultiplexer.CloseAsync();
                await connectionMultiplexer.DisposeAsync();
            }

            await redisContainer.StopAsync();
            await redisContainer.DisposeAsync();
        }

        [Fact]
        public async Task GetAsync_ReturnsValue_WhenKeyExists()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = new TestObject { Id = 1, Name = "Test" };

            await redisService.SetAsync(key, expectedValue);

            // Act
            var result = await redisService.GetAsync<TestObject>(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedValue.Id, result.Id);
            Assert.Equal(expectedValue.Name, result.Name);
        }

        [Fact]
        public async Task GetAsync_ReturnsDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "non-existent-key";

            // Act
            var result = await redisService.GetAsync<TestObject>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_SetsValue_Correctly()
        {
            // Arrange
            var key = "test-key";
            var value = new TestObject { Id = 1, Name = "Test" };
            var expiry = TimeSpan.FromMinutes(10);

            // Act
            // Act
            await redisService.SetAsync(key, value, expiry);

            // Assert
            var db = connectionMultiplexer.GetDatabase();
            var redisValue = await db.StringGetAsync(key);
            Assert.True(redisValue.HasValue);
            Assert.Contains("Test", redisValue.ToString());
        }

        [Fact]
        public async Task RemoveAsync_RemovesKey_Correctly()
        {
            // Arrange
            var key = "test-key";
            var db = connectionMultiplexer.GetDatabase();
            await db.StringSetAsync(key, "some-value");

            // Act
            await redisService.RemoveAsync(key);

            // Assert
            var exists = await db.KeyExistsAsync(key);
            Assert.False(exists);
        }

        public class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
