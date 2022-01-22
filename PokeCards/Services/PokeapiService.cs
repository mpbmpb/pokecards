using System.Text.RegularExpressions;
using PokeCards.Contracts.Responses;
using PokeCards.Data;

namespace PokeCards.Services;

public class PokeapiService
{
    private const string _speciesUrl = "https://pokeapi.co/api/v2/pokemon-species?limit=100000";
    private readonly IHttpClientFactory _clientFactory;
    
    private static readonly Regex PokemonIdFromUrl = new Regex(@"\/(?<id>\d+)\/", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
    private static string GetIdFromUrl(string url) => PokemonIdFromUrl.Match(url).Groups["id"].Value;

    private List<Pokemon> _pokemons = new();

    private int[][] _generations = new int[][] { new[] { 0 }, // generation 0 does not have a populationCount
        new[] {0, 151 }, new []{151, 100}, new []{251, 135}, // { where the generation starts, how many pokemon in the generation }
        new []{386, 107}, new []{493, 156}, new []{649, 72}, 
        new []{721, 88}, new []{809, 89}, new []{897, 10_000} };  // last gen 9 is called 8+ meaning everything after gen 8

    public (int generation,int offset, int populationCount) GenerationRange(int gen) => gen > 0 && gen < _generations.Length ?
        (gen, _generations[gen][0], _generations[gen][1]) : (1, 0, 151);

    public PokeapiService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<List<Pokemon>> GetAllPokemonAsync()
    {
        if (_pokemons.Count == 0)
        {
            await GetAllPokemonFromApiAsync();
        }

        return _pokemons;
    }

    public async Task<Pokemon> GetPokemonAsync(string id)
    {
        if (_pokemons.Count == 0)
        {
            await GetAllPokemonFromApiAsync();
        }

        return _pokemons.FirstOrDefault(p => p.Id == id) ?? new();
    }


    private async Task GetAllPokemonFromApiAsync()
    {
        var species = new List<SpeciesResponse>();
        var result = new List<Pokemon>();

        var request = new HttpRequestMessage(HttpMethod.Get, _speciesUrl);
        var client = _clientFactory.CreateClient();
        var response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<PokeapiResponse>();
                species = apiResponse?.Results ?? new();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        foreach (var specimen in species)
        {
            result.Add(new Pokemon(GetIdFromUrl(specimen.url), specimen.name));
        }

        _pokemons = result;
    }
}