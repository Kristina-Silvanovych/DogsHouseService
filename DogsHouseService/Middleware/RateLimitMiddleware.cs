using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace DogsHouseService.Middleware
{
    public class RateLimitOptions
    {
        public int RequestsPerSecond { get; set; } = 10;
    }
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitOptions _options;
        private static readonly ConcurrentDictionary<string, TokenBucket> Buckets = new();

        public RateLimitMiddleware(RequestDelegate next, IOptions<RateLimitOptions> options)
        {
            _next = next;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var key = "global";
            var bucket = Buckets.GetOrAdd(key, _ => new TokenBucket(_options.RequestsPerSecond, TimeSpan.FromSeconds(1)));

            if (!bucket.TryConsume())
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "1";
                await context.Response.WriteAsync("Too many requests");
                return;
            }

            await _next(context);
        }

        private class TokenBucket
        {
            private readonly int _capacity;
            private readonly TimeSpan _refillPeriod;
            private double _tokens;
            private DateTime _lastRefill;
            private readonly object _lock = new();

            public TokenBucket(int capacity, TimeSpan refillPeriod)
            {
                _capacity = capacity;
                _refillPeriod = refillPeriod;
                _tokens = capacity;
                _lastRefill = DateTime.UtcNow;
            }

            public bool TryConsume()
            {
                lock (_lock)
                {
                    Refill();
                    if (_tokens >= 1)
                    {
                        _tokens -= 1;
                        return true;
                    }
                    return false;
                }
            }

            private void Refill()
            {
                var now = DateTime.UtcNow;
                var seconds = (now - _lastRefill).TotalSeconds;
                if (seconds <= 0) return;
                var tokensToAdd = seconds * (_capacity / _refillPeriod.TotalSeconds);
                if (tokensToAdd > 0)
                {
                    _tokens = Math.Min(_capacity, _tokens + tokensToAdd);
                    _lastRefill = now;
                }
            }
        }
    }

    public static class RateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseSimpleRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }
    }
}
