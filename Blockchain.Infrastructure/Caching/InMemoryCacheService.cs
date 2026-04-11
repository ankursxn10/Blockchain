using System.Text.Json;

namespace Blockchain.Infrastructure.Caching
{
    public class InMemoryCacheService : ICacheService
    {
        private static readonly Dictionary<string, (object Value, DateTime Expiration)> Cache = new();
        private readonly object _lockObject = new();

        public Task<T?> GetAsync<T>(string key)
        {
            lock (_lockObject)
            {
                if (Cache.TryGetValue(key, out var item))
                {
                    if (item.Expiration > DateTime.UtcNow)
                    {
                        return Task.FromResult((T?)item.Value);
                    }

                    Cache.Remove(key);
                }
            }

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            lock (_lockObject)
            {
                var expirationTime = expiration.HasValue
                    ? DateTime.UtcNow.Add(expiration.Value)
                    : DateTime.UtcNow.AddHours(1);

                Cache[key] = (value!, expirationTime);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            lock (_lockObject)
            {
                Cache.Remove(key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            lock (_lockObject)
            {
                var keysToRemove = Cache.Keys.Where(k => k.Contains(pattern)).ToList();
                foreach (var key in keysToRemove)
                {
                    Cache.Remove(key);
                }
            }

            return Task.CompletedTask;
        }
    }
}
