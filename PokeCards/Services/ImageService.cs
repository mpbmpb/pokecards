using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using PokeCards.Data;

namespace PokeCards.Services;

public class ImageService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly MemoryCache _cache;
    private static readonly Regex ExtensionFromUrl = new Regex(@"\.(?<ext>\w{3})$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

    public ImageService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 1024
        });
    }

    public async Task GetImagesAsync(List<Card> cards)
    {
        var sw = Stopwatch.StartNew();
        var client = _clientFactory.CreateClient("ImageService");
        var tasks = new List<Task>();
        foreach (var card in cards)
        {
            tasks.Add(Task.Run(async () => 
            {
                try
                {
                    card.ImageBase64String = await GetImageAsync(card.ImageUrl, client);
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

    public async Task<string> GetImageAsync(string url)
    {
        var extension = ExtensionFromUrl.Match(url).Groups["ext"].Value;
        if (extension == "png")
            return await GetPngImageAsync(url);
        if (extension == "jpg")
            return await GetJpgImageAsync(url);
        return "";
    }

    private async Task<string> GetImageAsync(string url, HttpClient client)
    {
        var extension = ExtensionFromUrl.Match(url).Groups["ext"].Value;
        if (extension == "png")
            return await GetPngImageAsync(url, client);
        if (extension == "jpg")
            return await GetJpgImageAsync(url, client);
        return "";
    }

   public async Task<string> GetPngImageAsync(string url)
    {
        var client = _clientFactory.CreateClient("ImageService");
        if (!_cache.TryGetValue(url, out string imageBase64String))
        {
            var imageBytes = await client.GetByteArrayAsync(url);
            imageBase64String = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
            _cache.Set(url, imageBase64String, new MemoryCacheEntryOptions().SetSize(1));
        }
        return imageBase64String;
    }

   private async Task<string> GetPngImageAsync(string url, HttpClient client)
    {
        if (!_cache.TryGetValue(url, out string imageBase64String))
        {
            var imageBytes = await client.GetByteArrayAsync(url);
            imageBase64String = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
            _cache.Set(url, imageBase64String, new MemoryCacheEntryOptions().SetSize(1));
        }
        return imageBase64String;
    }
   
    public async Task<string> GetJpgImageAsync(string url)
    {
        var client = _clientFactory.CreateClient("ImageService");
        if (!_cache.TryGetValue(url, out string imageBase64String))
        {
            var imageBytes = await client.GetByteArrayAsync(url);
            imageBase64String = $"data:image/jpg;base64,{Convert.ToBase64String(imageBytes)}";
            _cache.Set(url, imageBase64String, new MemoryCacheEntryOptions().SetSize(1));
        }
        return imageBase64String;
    }

    private async Task<string> GetJpgImageAsync(string url, HttpClient client)
    {
        if (!_cache.TryGetValue(url, out string imageBase64String))
        {
            var imageBytes = await client.GetByteArrayAsync(url);
            imageBase64String = $"data:image/jpg;base64,{Convert.ToBase64String(imageBytes)}";
            _cache.Set(url, imageBase64String, new MemoryCacheEntryOptions().SetSize(1));
        }
        return imageBase64String;
    }

}