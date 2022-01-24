using System.Diagnostics;
using PokeCards.Contracts.Responses;
using PokeCards.Data;

namespace PokeCards.Services;

public class PokemontcgService
{
    private PokeapiService _pokeapiService;
    private const string CardsBaseUrl = "https://api.pokemontcg.io/v2/cards?q=nationalPokedexNumbers:";
    private const int MaxPageSize = 250;
    public const int MaxParallelRequests = 60;
    private readonly IHttpClientFactory _clientFactory;
    private PokemontcgResponse _apiResponse = new();
    private List<Card> _cards = new();
    private Object _padLock = new();

    public PokemontcgService(IHttpClientFactory clientFactory, PokeapiService pokeapiService)
    {
        _clientFactory = clientFactory;
        _pokeapiService = pokeapiService;
    }

    public async Task<List<Card>> GetAllCardsWithAsync(int speciesId)
    {
        var sw = new Stopwatch();
        sw.Start();
        _cards = new();
        var totalCount = 80;
        var pageSize = 16;
        var page = 1;
        
        while (_cards.Count < totalCount)
        {
            var tasks = new List<Task>();
            var requests = (totalCount - _cards.Count) / pageSize;
            if (totalCount % pageSize != 0)
                requests++;

            for (int i = 0; i < requests; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var response = await GetCardsPageForAsync(speciesId, pageSize, page++);
                    await ExtractCards(response);
                    Console.WriteLine($"cards in memory: {_cards.Count}");
                }));
            }
            await Task.WhenAll(tasks);
        }
        sw.Stop();

        

        async Task ExtractCards(HttpResponseMessage httpResponseMessage)
        {
            var parsed = await TryParseResponseAsync(httpResponseMessage);
            if (parsed)
            {
                await AddCardsAsync(_apiResponse.Data ?? Array.Empty<PokemontcgCardResponse>());
                if (_apiResponse.TotalCount != totalCount)
                    Interlocked.Exchange(ref totalCount, _apiResponse.TotalCount ?? totalCount);
            }
        }

        Console.WriteLine($"Total time to get cards: {sw.ElapsedMilliseconds} ms");
        return _cards;
    }

    public async Task GetAllCardsAsync()
    {
        _cards = new();
        var totalCount = 14410;
        var page = 1;
        

        while (_cards.Count < totalCount)
        {
            var tasks = new List<Task>();
            var requests = Math.Min((totalCount - _cards.Count) / MaxPageSize, MaxParallelRequests);
            if (totalCount % MaxPageSize != 0)
                requests++;

            for (int i = 0; i < requests; i++)
            {
                tasks.Add(Task.Run( async () =>
                {
                    var response = await GetCardsPageAsync(page++);
                    await ExtractCards(response);
                    Console.WriteLine($"cards in memory: {_cards.Count}");
                }));
            }
            await Task.WhenAll(tasks);
        }

        

        async Task ExtractCards(HttpResponseMessage httpResponseMessage)
        {
            var parsed = await TryParseResponseAsync(httpResponseMessage);
            if (parsed)
            {
                await AddCardsAsync(_apiResponse.Data ?? Array.Empty<PokemontcgCardResponse>());
                Interlocked.Exchange(ref totalCount, _apiResponse.TotalCount ?? totalCount);
            }
        }
    }

    private async Task<HttpResponseMessage> GetCardsPageAsync(int page)
    {
        var baseUrl = $"https://api.pokemontcg.io/v2/cards?pageSize={MaxPageSize}&page=";
        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl + page);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);
        return response;
    }

 private async Task<HttpResponseMessage> GetCardsPageForAsync(int id, int pageSize,int page)
    {
        var baseUrl = $"https://api.pokemontcg.io/v2/cards?q=nationalPokedexNumbers:{id}&pageSize={pageSize}&page={page}";
        var request = new HttpRequestMessage(HttpMethod.Get, baseUrl);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);
        return response;
    }

    private async Task<bool> TryParseResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) 
            return false;
        
        try
        {
            _apiResponse = await response.Content.ReadFromJsonAsync<PokemontcgResponse>() ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return true;
    }

    private async Task AddCardsAsync(IEnumerable<PokemontcgCardResponse> cardResponses)
    {
        var pokemons = await _pokeapiService.GetAllPokemonAsync();
        foreach (var cardResponse in cardResponses)
        {
            lock (_padLock)
            {
                _cards.Add(new Card
                {
                    Id = cardResponse.Id ?? "",
                    Name = cardResponse.Name ?? "",
                    Supertype = cardResponse.Supertype ?? "",
                    Types = cardResponse.Types ?? Array.Empty<string>(),
                    Pokemons = cardResponse.PokedexNumbers?.Select(id => 
                            pokemons.FirstOrDefault(p => p.Id == id) ?? new(0) )
                        .Where(p => p.Id != 0).ToList() ?? new(),
                    ImageUrl = cardResponse.Images?.small ?? cardResponse.Images?.large ?? ""
                });
            }
        }
    }
}