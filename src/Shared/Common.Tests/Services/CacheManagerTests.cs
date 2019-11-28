using System;
using System.Threading.Tasks;
using AutoFixture;
using Fanex.Caching;
using LiveScore.Common.Services;
using NSubstitute;
using Xunit;

namespace LiveScore.Common.Tests.Services
{
    public class CacheManagerTests
    {
        private readonly ICacheManager cacheManager;
        private readonly ILoggingService loggingService;
        private readonly Fixture Random;
        private readonly string CacheKey;

        public CacheManagerTests()
        {
            loggingService = Substitute.For<ILoggingService>();
            Random = new Fixture();

            CacheKey = Random.Create<string>();
        }

        [Fact]
        public async Task GetOrSetAsync_CacheKeyIsNullOrWhiteSpace_ReturnsDefault()
        {
            // Arrange
            var factory = Substitute.For<Func<Task<object>>>();

            // Act

            // Assert
            Assert.Null(actual);
        }

        [Fact]
        public async Task GetOrSetAsync_ForceFetchLatestDataIsTrue_AlwaysInvokeFactory()
        {
            // Arrange
            var factory = Substitute.For<Func<Task<object>>>();

            // Act
            await cacheManager.GetOrSetAsync(CacheKey, factory, cacheItemOptions, forceFetchLatestData: true).ConfigureAwait(false);

            // Assert
            await factory.Received().Invoke().ConfigureAwait(false);
        }

        [Fact]
        public async Task GetOrSetAsync_ForceFetchLatestDataIsTrue_AlwaysInvokeCachingSetAsyncWithValueGetFromFactory()
        {
            // Arrange
            var dataReturnFromFactory = Random.Create<int>();
            Task<int> factory() => Task.FromResult(dataReturnFromFactory);

            // Act
            await cacheManager.GetOrSetAsync(CacheKey, factory, cacheItemOptions, forceFetchLatestData: true).ConfigureAwait(false);

            // Assert
            await cacheService.Received().SetAsync(CacheKey, dataReturnFromFactory, cacheItemOptions).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetOrSetAsync_ForceFetchLatestDataIsFalse_CacheServiceInvokesGetAsync()
        {
            // Arrange
            Task<int> factory() => Task.FromResult(Random.Create<int>());

            // Act
            await cacheManager.GetOrSetAsync(CacheKey, factory, cacheItemOptions, forceFetchLatestData: false).ConfigureAwait(false);

            // Assert
            await cacheService
                .Received()
                .GetAsync<int>(CacheKey)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetOrSetAsync_DataGetFromCacheIsNull_InvokeFactory()
        {
            // Arrange
            var factory = Substitute.For<Func<Task<object>>>();
            object cacheData = null;
            cacheService.GetAsync<object>(CacheKey).Returns(Task.FromResult(cacheData));

            // Act
            await cacheManager
                .GetOrSetAsync(CacheKey, factory, cacheItemOptions, forceFetchLatestData: false)
                .ConfigureAwait(false);

            // Assert
            await factory
                .Received()
                .Invoke()
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetOrSetAsync_DataIsInCache_ReturnDataGetFromCache()
        {
            // Arrange
            var value = Random.Create<object>();
            var factory = Substitute.For<Func<Task<object>>>();
            cacheService.GetAsync<object>(CacheKey).Returns(Task.FromResult(value));

            // Act
            var actual = await cacheManager
                .GetOrSetAsync(CacheKey, factory, cacheItemOptions, forceFetchLatestData: false)
                .ConfigureAwait(false);

            // Assert
            Assert.Equal(value, actual);
        }

        [Fact]
        public async Task GetOrSetAsync_TaskCanceledExceptionIsThrown_InvokeLogExceptionAsync()
        {
            // Arrange
            var exception = new TaskCanceledException();
            Task<int> factory() => throw exception;

            // Act
            await cacheManager
                .GetOrSetAsync(CacheKey, factory, cacheItemOptions, forceFetchLatestData: true)
                .ConfigureAwait(false);

            // Assert
            await loggingService
                .Received()
                .LogExceptionAsync(exception)
                .ConfigureAwait(false);
        }
    }
}