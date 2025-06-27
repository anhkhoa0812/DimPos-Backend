using DimPos.Basket.Application.Models;
using StackExchange.Redis;

namespace DimPos.Basket.Application.Services.Interface;

public interface IRedisService
{
    Task RemoveFromListAsync(string key, string value);

    Task<List<string>> GetListAsync(string key);

    Task SetHashAsync(string key, string field, string value);
    Task<string?> GetHashAsync(string key, string field);
    Task<List<string>> GetSortedSetAsync(string key);
    Task SetSortedSetAsync(string key, string member, double score);
    Task RemoveHashAsync(string key, string member);
    Task RemoveSortedSetAsync(string key, string member);
}