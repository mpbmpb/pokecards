﻿using System.Collections.Concurrent;

namespace PokeCards.Services.simplecache;

public class MemoryCache<T>

{
    private readonly ConcurrentDictionary<object, T> _dictionary;
    private ConcurrentQueue<object> _keyQueue { get; set; } = new();
    private CacheOptions _options { get; init; } = new();

    public MemoryCache()
    {
        _dictionary = new();
    }

    public MemoryCache(CacheOptions options)
        : this()
    {
        _options = options;
    }
    
    public bool Set(object key, T value)
    {
        if (_dictionary.Count >= _options.SizeLimit)
        {
            var pruned = TryPrune();
            if (!pruned)
                return false;
        }
        var success =_dictionary.TryAdd(key, value);
        if (success)
            _keyQueue.Enqueue(key);
        
        return success;
    }

    public bool TryGetValue(object key, out T value)
    {
        var success = _dictionary.TryGetValue(key, out value!);
        if (success && _options.EvictionPolicy == Evict.LeastRecentlyUsed)
            Refresh(key);
        
        return success;
    }

    private void Refresh(object key)
    {
        _keyQueue = new ConcurrentQueue<object>(_keyQueue.Where(x => !x.Equals(key)));
        _keyQueue.Enqueue(key);
    }

    public bool TryPrune()
    {
        _keyQueue.TryDequeue(out var key);
        return _dictionary.TryRemove(key, out _);
    }
}