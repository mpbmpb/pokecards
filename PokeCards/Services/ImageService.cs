using System.Diagnostics;
using System.Text.RegularExpressions;
using PokeCards.Data;
using PokeCards.Services.simplecache;

namespace PokeCards.Services;

public class ImageService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly MemoryCache<byte[]> _cache;
    private readonly byte[] _notfoundImage;
    private static readonly Regex ExtensionFromUrl = new Regex(@"\.(?<ext>\w{3})$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

    public ImageService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _cache = new (new CacheOptions
        {
            SizeLimit = 250,
            EvictionPolicy = Evict.LeastRecentlyUsed
        });
        _notfoundImage = File.ReadAllBytes("wwwroot/Images/Logos/notfound.png");
    }

    public async Task GetImagesAsync(List<Card> cards)
    {
        var sw = Stopwatch.StartNew();
        using var client = _clientFactory.CreateClient("ImageService");
        var tasks = new List<Task>();
        foreach (var card in cards)
        {
            tasks.Add(Task.Run(async () => 
            {
                try
                {
                    card.ImageType = ExtensionFromUrl.Match(card.ImageUrl).Groups["ext"].Value;
                    card.ImageBytes = await GetImageAsync(card.ImageUrl, client);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }));
        }
        await Task.WhenAll(tasks);
        sw.Stop();
        Console.WriteLine($"Time to get images: {sw.ElapsedMilliseconds} ms");
    }

    private async Task<byte[]> GetImageAsync(string url,HttpClient client)
    {
        if (!_cache.TryGetValue(url, out byte[] imageBytes))
        {
            try
            {
                imageBytes = await client.GetByteArrayAsync(url);
                _cache.Set(url, imageBytes);
            }
            catch (Exception)
            {
                imageBytes = _notfoundImage;
            }
        }
        return imageBytes;
    }
}