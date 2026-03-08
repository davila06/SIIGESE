using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Services
{
    /// <summary>
    /// Stores revoked JWT tokens as SHA-256 hashed keys in IDistributedCache.
    /// Uses Redis in production or the in-memory fallback in development.
    /// </summary>
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDistributedCache _cache;
        private const string Prefix = "blacklisted_token:";

        public TokenBlacklistService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task BlacklistTokenAsync(string token, TimeSpan ttl)
        {
            var key = BuildCacheKey(token);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };
            await _cache.SetStringAsync(key, "1", options);
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            var key = BuildCacheKey(token);
            var value = await _cache.GetStringAsync(key);
            return value is not null;
        }

        /// <summary>
        /// Hashes the full JWT so the cache key is a fixed-length hex string
        /// rather than a potentially large JWT value.
        /// </summary>
        private static string BuildCacheKey(string token)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return $"{Prefix}{Convert.ToHexString(hash)}";
        }
    }
}
