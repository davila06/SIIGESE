namespace Application.Interfaces
{
    /// <summary>
    /// Service for managing JWT token revocation.
    /// Backed by IDistributedCache (Redis in production, in-memory in development).
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>Adds a token to the blacklist with a given TTL.</summary>
        Task BlacklistTokenAsync(string token, TimeSpan ttl);

        /// <summary>Returns true if the token has been revoked.</summary>
        Task<bool> IsBlacklistedAsync(string token);
    }
}
