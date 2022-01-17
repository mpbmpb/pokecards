namespace PokeCards.Services;

public class ImageFetchingService
{
    private readonly IHttpClientFactory _clientFactory;

    public ImageFetchingService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    
}