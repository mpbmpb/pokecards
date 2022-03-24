namespace PokeCards.Services.simplecache;

public class CacheOptions
{
    public int? SizeLimit { get; set; }
    public Evict EvictionPolicy { get; set; }
}