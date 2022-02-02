using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using PokeCards.Contracts.Responses;
using PokeCards.Data;

namespace PokeCards.Services;

public class PokemontcgService
{
    private readonly PokeapiService _pokeapiService;
    private readonly IHttpClientFactory _clientFactory;
    private readonly MemoryCache _cache;
    private List<Card> _cards = new();
    private readonly object _padLock = new();
    private const int _pageSize = 35;

    public PokemontcgService(IHttpClientFactory clientFactory, PokeapiService pokeapiService)
    {
        _clientFactory = clientFactory;
        _pokeapiService = pokeapiService;
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 20
        });
    }

    public async Task<List<Card>> GetAllCardsForAsync(int speciesId)
    {
        var sw = new Stopwatch();
        sw.Start();

        if (_cache.TryGetValue(speciesId, out List<Card> cards))
        {
            sw.Stop();
            Console.WriteLine($"Total time to get cards: {sw.ElapsedMilliseconds} ms");
            return cards;
        }

        _cards = new();
        
        using var client = _clientFactory.CreateClient("Pokemontcg");

        var success = true;
        var totalCount = 140;
        var startPage = 1;

        while (_cards.Count < totalCount && success)
        {
            var pages = (totalCount - _cards.Count) / _pageSize;
            if (totalCount % _pageSize != 0)
                pages++;
            
            var responses = await SendRequestsParallelAsync(client, speciesId, pages, startPage);
            foreach (var response in responses)
            {
                if (!response!.IsSuccessStatusCode)
                {
                    success = false;
                    continue;
                }
                var responseCount =  await ExtractCards(response);
                totalCount = responseCount;
                response.Dispose();
            }

            startPage += pages;
        }

        if (success && _cards.Count > 0 && _cards.Count == totalCount)
            _cache.Set(speciesId, _cards, new MemoryCacheEntryOptions().SetSize(1));
        
        sw.Stop();
        Console.WriteLine($"Total time to get cards: {sw.ElapsedMilliseconds} ms");
        return _cards;
    }

    private async Task<List<HttpResponseMessage?>> SendRequestsParallelAsync(HttpClient client, int speciesId, int pages, int startPage)
    {
        var tasks = new Task[pages];
        var responses = new List<HttpResponseMessage?>();

        for (int i = 0; i < pages; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                var response = await GetCardsPageForAsync(speciesId, startPage++, client);
                lock (_padLock)
                {
                    responses.Add(response);
                }
            });
        }

        await Task.WhenAll(tasks);
        return responses;
    }

    private async Task<HttpResponseMessage> GetCardsPageForAsync(int id, int page, HttpClient client)
    {
        var url = $"https://api.pokemontcg.io/v2/cards?q=nationalPokedexNumbers:{id}&pageSize={_pageSize}&page={page}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(request);
        return response;
    }

    private async Task<int> ExtractCards(HttpResponseMessage httpResponseMessage)
    {
        var (success, tcgResponse) = await TryParseResponseAsync(httpResponseMessage);
        if (!success)
            return 0;
            
        await AddCardsAsync(tcgResponse!.Data);
        return tcgResponse.TotalCount ?? 0;
    }

    private async Task<(bool, PokemontcgResponse?)> TryParseResponseAsync(HttpResponseMessage response)
    {
        PokemontcgResponse tcgResponse;
        try
        {
            tcgResponse = await response.Content.ReadFromJsonAsync<PokemontcgResponse>() ?? new();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false, null);
        }
        return (true, tcgResponse);
    }

    private async Task AddCardsAsync(IEnumerable<PokemontcgCardResponse>? cardResponses)
    {
        if (cardResponses is null)
            return;
        
        var pokemons = await _pokeapiService.GetAllPokemonAsync();
        foreach (var cardResponse in cardResponses)
        {
            _cards.Add(new Card
            {
                Id = cardResponse.Id ?? "",
                Name = cardResponse.Name ?? "",
                Supertype = cardResponse.Supertype ?? "",
                Types = cardResponse.Types ?? Array.Empty<string>(),
                Pokemons = cardResponse.PokedexNumbers?.Select(id => 
                        pokemons.FirstOrDefault(p => p.Id == id) ?? new() )
                    .Where(p => p.Id != 0).ToList() ?? new(),
                ImageUrl = cardResponse.Images?.small ?? cardResponse.Images?.large ?? ""
            });
        }
    }
}