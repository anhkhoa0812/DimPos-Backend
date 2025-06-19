using DimPos.Basket.Application.Services.Interface;
using StackExchange.Redis;

namespace DimPos.Basket.Application.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _db;
    private readonly IConnectionMultiplexer _redisConnection;
    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
        _redisConnection = redis;
    }
    public async Task<string?> GetStringAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public async Task<bool> RemoveKeyAsync(string key)
    {
        return await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await _db.StringSetAsync(key, value, expiry);
    }
    
    public async Task PushToListAsync(string key, string value)
    {
        await _db.ListRightPushAsync(key, value);
    }

    public async Task RemoveFromListAsync(string key, string value)
    {
        await _db.ListRemoveAsync(key, value, 1);
    }

    public Task<List<string>> GetListAsync(string key)
    {
        return _db.ListRangeAsync(key).ContinueWith(t => t.Result.Select(x => x.ToString()).ToList());
    }

    public async Task SetHashAsync(string key, string field, string value)
    {
        await _db.HashSetAsync(key, field, value);
    }

    public async Task<string?> GetHashAsync(string key, string field)
    {
        return await _db.HashGetAsync(key, field);
    }
    
    public Task<List<string>> GetSortedSetAsync(string key)
    {
        return _db.SortedSetRangeByRankAsync(key, order: Order.Descending)
            .ContinueWith(t => t.Result.Select(x => x.ToString()).ToList());
    }

    public async Task SetSortedSetAsync(string key, string member, double score)
    {
        await _db.SortedSetAddAsync(key, member, score);
    }

    public async Task RemoveHashAsync(string key, string member)
    {
        await _db.HashDeleteAsync(key, member);
    }

    public Task RemoveSortedSetAsync(string key, string member)
    {
        return _db.SortedSetRemoveAsync(key, member);
    }
}