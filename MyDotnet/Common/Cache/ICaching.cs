﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace MyDotnet.Common.Cache;

/// <summary>
/// 缓存抽象接口,基于IDistributedCache封装
/// </summary>
public interface ICaching
{
	public IDistributedCache Cache { get; }

	bool Exists(string cacheKey);
	Task<bool> ExistsAsync(string cacheKey);

	T Get<T>(string cacheKey);
	Task<T> GetAsync<T>(string cacheKey);

	object Get(Type type, string cacheKey);
	Task<object> GetAsync(Type type, string cacheKey);

	string GetString(string cacheKey);
	Task<string> GetStringAsync(string cacheKey);

	void Remove(string key);
	Task RemoveAsync(string key);

	void Set<T>(string cacheKey, T value, TimeSpan? expire = null);
	Task SetAsync<T>(string cacheKey, T value);
	Task SetAsync<T>(string cacheKey, T value, TimeSpan expire);

	void SetPermanent<T>(string cacheKey, T value);
	Task SetPermanentAsync<T>(string cacheKey, T value);

	void SetString(string cacheKey, string value, TimeSpan? expire = null);
	Task SetStringAsync(string cacheKey, string value);
	Task SetStringAsync(string cacheKey, string value, TimeSpan expire);
}