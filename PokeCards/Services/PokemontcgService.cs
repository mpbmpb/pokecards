using PokeCards.Contracts.Responses;
using PokeCards.Data;

namespace PokeCards.Services;

public class PokemontcgService
{
    private const string _cardsBaseUrl = "https://api.pokemontcg.io/v2/cards?q=nationalPokedexNumbers:";
    private readonly IHttpClientFactory _clientFactory;
    private PokemontcgResponse _apiResponse = new();

    public PokemontcgService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }
    
    public async Task<List<Card>> GetAllCardsWithAsync(string speciesId)
    {
        var result = new List<Card>();

        var request = new HttpRequestMessage(HttpMethod.Get, _cardsBaseUrl + speciesId);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                _apiResponse = await response.Content.ReadFromJsonAsync<PokemontcgResponse>() ?? new();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        foreach (var cardResponse in _apiResponse.Data ?? Array.Empty<PokemontcgCardResponse>())
        {
            result.Add(new Card
            {
                Id = cardResponse.Id ?? "",
                Name = cardResponse.Name ?? "",
                Supertype = cardResponse.Supertype ?? "",
                Types = cardResponse.Types ?? Array.Empty<string>(),
                ImageUrl = cardResponse.Images?.small ?? cardResponse.Images?.large ?? ""
            });
        }

        return result;
    }
}