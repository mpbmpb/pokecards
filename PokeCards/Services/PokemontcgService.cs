using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using PokeCards.Contracts.Responses;
using PokeCards.Data;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Wrap;

namespace PokeCards.Services;

public class PokemontcgService
{
    private PokeapiService _pokeapiService;
    private const string CardsBaseUrl = "https://api.pokemontcg.io/v2/cards?q=nationalPokedexNumbers:";
    private const int MaxPageSize = 250;
    public const int MaxParallelRequests = 60;
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

    private static AsyncPolicyWrap<HttpResponseMessage> GetHttpPolicy()
    {
        var fallBackPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
                !r.IsSuccessStatusCode).Or<Exception>()
            .FallbackAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5);
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);
        var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode).Or<Exception>()
            .WaitAndRetryAsync(delay);
        var circuitBreaker = Policy.HandleResult<HttpResponseMessage>(r =>
                !r.IsSuccessStatusCode).Or<Exception>()
            .AdvancedCircuitBreakerAsync(0.7, TimeSpan.FromSeconds(10), 8, TimeSpan.FromMinutes(1));

        var policy = fallBackPolicy.WrapAsync(retryPolicy).WrapAsync(timeoutPolicy).WrapAsync(circuitBreaker);
        return policy;
    }

    public async Task<List<Card>> GetAllCardsWithAsync(int speciesId)
    {
        var sw = new Stopwatch();
        sw.Start();
        _cards = new List<Card>();
        var responses = new List<HttpResponseMessage?>();
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

            for (int i = 0; i < requests; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var response = await GetCardsPageForAsync(speciesId, pageSize, page++);
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

    private async Task<HttpResponseMessage> GetCardsPageForAsync(int id, int pageSize,int page)
    {
        var url = $"https://api.pokemontcg.io/v2/cards?q=nationalPokedexNumbers:{id}&pageSize={pageSize}&page={page}";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var client = _clientFactory.CreateClient();
        var response = await _policy.ExecuteAsync(async () => await client.SendAsync(request));
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