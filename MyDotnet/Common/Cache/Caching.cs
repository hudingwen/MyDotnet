
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace MyDotnet.Common.Cache;

public class Caching : ICaching
{
	private readonly IDistributedCache _cache;

	public Caching(IDistributedCache cache)
	{
		_cache = cache;
	}

	private byte[] GetBytes<T>(T source)
	{
		return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(source));
	}

	public IDistributedCache Cache => _cache;




	public bool Exists(string cacheKey)
	{
		var res = _cache.Get(cacheKey);
		return res != null;
	}

	/// <summary>
	/// 检查给定 key 是否存在
	/// </summary>
	/// <param name="cacheKey">键</param>
	/// <returns></returns>
	public async Task<bool> ExistsAsync(string cacheKey)
	{
		var res = await _cache.GetAsync(cacheKey);
		return res != null;
	}


	public T Get<T>(string cacheKey)
	{
		var res = _cache.Get(cacheKey);
		return res == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(res));
	}

	/// <summary>
	/// 获取缓存
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="cacheKey"></param>
	/// <returns></returns>
	public async Task<T> GetAsync<T>(string cacheKey)
	{
		var res = await _cache.GetAsync(cacheKey);
		return res == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(res));
	}

	public object Get(Type type, string cacheKey)
	{
		var res = _cache.Get(cacheKey);
		return res == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(res), type);
	}

	public async Task<object> GetAsync(Type type, string cacheKey)
	{
		var res = await _cache.GetAsync(cacheKey);
		return res == null ? default : JsonConvert.DeserializeObject(Encoding.UTF8.GetString(res), type);
	}

	public string GetString(string cacheKey)
	{
		return _cache.GetString(cacheKey);
	}

	/// <summary>
	/// 获取缓存
	/// </summary>
	/// <param name="cacheKey"></param>
	/// <returns></returns>
	public async Task<string> GetStringAsync(string cacheKey)
	{
		return await _cache.GetStringAsync(cacheKey);
	}

	public void Remove(string key)
	{
		_cache.Remove(key);
	}

	/// <summary>
	/// 删除缓存
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public async Task RemoveAsync(string key)
	{
		await _cache.RemoveAsync(key);
	}



	public void Set<T>(string cacheKey, T value, TimeSpan? expire = null)
	{
		_cache.Set(cacheKey, GetBytes(value),
			expire == null
				? new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)}
				: new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = expire});
	}

	/// <summary>
	/// 增加对象缓存
	/// </summary>
	/// <param name="cacheKey"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public async Task SetAsync<T>(string cacheKey, T value)
	{
		await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)),
			new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)});
	}

	/// <summary>
	/// 增加对象缓存,并设置过期时间
	/// </summary>
	/// <param name="cacheKey"></param>
	/// <param name="value"></param>
	/// <param name="expire"></param>
	/// <returns></returns>
	public async Task SetAsync<T>(string cacheKey, T value, TimeSpan expire)
	{
		await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)),
			new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = expire});
	}

	public void SetPermanent<T>(string cacheKey, T value)
	{
		_cache.Set(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
	}

	public async Task SetPermanentAsync<T>(string cacheKey, T value)
	{
		await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
	}

	public void SetString(string cacheKey, string value, TimeSpan? expire = null)
	{
		if (expire == null)
			_cache.SetString(cacheKey, value, new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
		else
			_cache.SetString(cacheKey, value, new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = expire});
	}

	/// <summary>
	/// 增加字符串缓存
	/// </summary>
	/// <param name="cacheKey"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public async Task SetStringAsync(string cacheKey, string value)
	{
		await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
	}

	/// <summary>
	/// 增加字符串缓存,并设置过期时间
	/// </summary>
	/// <param name="cacheKey"></param>
	/// <param name="value"></param>
	/// <param name="expire"></param>
	/// <returns></returns>
	public async Task SetStringAsync(string cacheKey, string value, TimeSpan expire)
	{
		await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions() {AbsoluteExpirationRelativeToNow = expire});
	}
}