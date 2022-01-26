using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using PokeCards.Contracts.Responses;
using PokeCards.Data;
using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Wrap;

namespace PokeCards.Services;

public class PokemontcgService
{
    private PokeapiService _pokeapiService;
    private readonly IHttpClientFactory _clientFactory;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _policy;
    private List<Card> _cards = new();
    private Object _padLock = new();

    public PokemontcgService(IHttpClientFactory clientFactory, PokeapiService pokeapiService)
    {
        _clientFactory = clientFactory;
        _pokeapiService = pokeapiService;
        _policy = GetHttpPolicy();
    }

    public static AsyncPolicyWrap<HttpResponseMessage> GetHttpPolicy()
    {
        var fallBackPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
                !r.IsSuccessStatusCode).Or<Exception>()
            .FallbackAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5);
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(8);
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<Exception>(e => e is not BrokenCircuitException)
            .WaitAndRetryAsync(delay);
        var circuitBreaker = Policy.HandleResult<HttpResponseMessage>(r =>
                !r.IsSuccessStatusCode).Or<Exception>()
            .AdvancedCircuitBreakerAsync(0.7, TimeSpan.FromSeconds(20), 12, TimeSpan.FromSeconds(40));

        var policy = fallBackPolicy.WrapAsync(retryPolicy).WrapAsync(timeoutPolicy).WrapAsync(circuitBreaker);
        return policy;
    }

    public async Task<List<Card>> GetAllCardsWithAsync(int speciesId)
    {
        var sw = new Stopwatch();
        sw.Start();
        _cards = new List<Card>();
        using var client = _clientFactory.CreateClient("Pokemontcg");
        
        var totalCount = 80;
        var pageSize = 16;
        var page = 1;
        var running = true;
        
        while (_cards.Count < totalCount && running)
        {
            var requests = (totalCount - _cards.Count) / pageSize;
            if (totalCount % pageSize != 0)
                requests++;
            var tasks = new Task[requests];
            var responses = new List<HttpResponseMessage?>();

            for (int i = 0; i < requests; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var response = await GetCardsPageForAsync(speciesId, pageSize, page++, client);
                    lock(_padLock)
                    {
                        responses.Add(response);
                    }
                });
            }
            await Task.WhenAll(tasks);
            foreach (var response in responses)
            {
                if (!response!.IsSuccessStatusCode)
                {
                    running = false;
                    continue;
                }
                var responseCount =  await ExtractCards(response!);
                totalCount = responseCount;
            }
        }
        
        sw.Stop();
        Console.WriteLine($"Total time to get cards: {sw.ElapsedMilliseconds} ms");
        return _cards;
    }

    private async Task<HttpResponseMessage> GetCardsPageForAsync(int id, int pageSize,int page, HttpClient client)
    {
        var url = $"https://api.pokemontcg.io/v2/cards?q=nationalPokedexNumbers:{id}&pageSize={pageSize}&page={page}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(request);
        return response;
    }

    private async Task<int> ExtractCards(HttpResponseMessage httpResponseMessage)
    {
        if (!httpResponseMessage.IsSuccessStatusCode)
            return 0;
            
        var (success, tcgResponse) = await TryParseResponseAsync(httpResponseMessage);
        if (!success)
            return 0;
            
        await AddCardsAsync(tcgResponse!.Data);
        return tcgResponse.TotalCount ?? 0;
    }

    private async Task<(bool,PokemontcgResponse?)> TryParseResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) 
            return (false,null);
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
                        pokemons.FirstOrDefault(p => p.Id == id) ?? new(0) )
                    .Where(p => p.Id != 0).ToList() ?? new(),
                ImageUrl = cardResponse.Images?.small ?? cardResponse.Images?.large ?? ""
            });
        }
    }
}