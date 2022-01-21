namespace PokeCards.Services;

public class ImageFetchingService
{
    private readonly IHttpClientFactory _clientFactory;

    public ImageFetchingService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<string> GetPngImage(string url)
    {
        var http = _clientFactory.CreateClient();
        var imageBytes = await http.GetByteArrayAsync(url);
        var imageBase64String = Convert.ToBase64String(imageBytes);
        return string.Format("data:image/png;base64,{0}", imageBase64String);
    }

}