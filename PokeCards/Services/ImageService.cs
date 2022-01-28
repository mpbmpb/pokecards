using System.Diagnostics;
using System.Text.RegularExpressions;
using PokeCards.Data;

namespace PokeCards.Services;

public class ImageService
{
    private readonly IHttpClientFactory _clientFactory;
    private static readonly Regex ExtensionFromUrl = new Regex(@"\.(?<ext>\w{3})$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));

    public ImageService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
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
                    card.ImageBase64String = await GetPngImageAsync(card.ImageUrl, client);
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
        return;
    }


    public async Task GetImagesChunkedAsync(List<Card> cards)
    {
        var sw = Stopwatch.StartNew();
        var client = _clientFactory.CreateClient("ImageService");
        var tasks = new List<Task>();
        var chunkSize = cards.Count / 4;
        if (cards.Count % 4 != 0)
            chunkSize++;
        
        foreach (var cardsChunk in cards.Chunk(chunkSize))
        {
            tasks.Add(Task.Run(async () => 
            {
                foreach (var card in cardsChunk)
                {
                    try
                    {
                        card.ImageBase64String = await GetPngImageAsync(card.ImageUrl);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
        sw.Stop();
        Console.WriteLine($"Time to get images: {sw.ElapsedMilliseconds} ms");
        return;
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

    public async Task<string> GetImageAsync(string url, HttpClient client)
    {
        var extension = ExtensionFromUrl.Match(url).Groups["ext"].Value;
        if (extension == "png")
            return await GetPngImageAsync(url, client);
        if (extension == "jpg")
            return await GetJpgImageAsync(url, client);
        return "";
    }

    public string GetImage(string url, HttpClient client)
    {
        var extension = ExtensionFromUrl.Match(url).Groups["ext"].Value;
        if (extension == "png")
            return GetPngImage(url, client);
        if (extension == "jpg")
            return "";
        return "";
    }

   public async Task<string> GetPngImageAsync(string url)
    {
        var client = _clientFactory.CreateClient("ImageService");
        var imageBytes = await client.GetByteArrayAsync(url);
        var imageBase64String = Convert.ToBase64String(imageBytes);
        return $"data:image/png;base64,{imageBase64String}";
    }
    public async Task<string> GetPngImageAsync(string url, HttpClient client)
    {
        var imageBytes = await client.GetByteArrayAsync(url);
        var imageBase64String = Convert.ToBase64String(imageBytes);
        return string.Format("data:image/png;base64,{0}", imageBase64String);
    }

   public string GetPngImage(string url, HttpClient client)
    {
        var imageBytes = client.GetByteArrayAsync(url).GetAwaiter().GetResult();
        var imageBase64String = Convert.ToBase64String(imageBytes);
        return string.Format("data:image/png;base64,{0}", imageBase64String);
    }

    public async Task<string> GetJpgImageAsync(string url)
    {
        var client = _clientFactory.CreateClient();
        var imageBytes = await client.GetByteArrayAsync(url);
        var imageBase64String = Convert.ToBase64String(imageBytes);
        return string.Format("data:image/jpg;base64,{0}", imageBase64String);
    }

    public async Task<string> GetJpgImageAsync(string url, HttpClient client)
    {
        var imageBytes = await client.GetByteArrayAsync(url);
        var imageBase64String = Convert.ToBase64String(imageBytes);
        return string.Format("data:image/jpg;base64,{0}", imageBase64String);
    }

}