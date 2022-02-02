using System.Text.RegularExpressions;
using PokeCards.Contracts.Responses;
using PokeCards.Data;

namespace PokeCards.Services;

public class PokeapiService
{
    private const string _allSpeciesUrl = "https://pokeapi.co/api/v2/pokemon-species?limit=10000";
    private const string _speciesUrl = "https://pokeapi.co/api/v2/pokemon-species/";
    private readonly IHttpClientFactory _clientFactory;
    private static readonly Regex PokemonIdFromUrl = new Regex(@"\/(?<id>\d+)\/", RegexOptions.Compiled, TimeSpan.FromMilliseconds(250));
    private List<Pokemon> _pokemons = new();

    public PokeapiService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    private readonly int[][] _generations = new int[][] { new[] { 0 }, // generation 0 does not have a populationCount
        new[] {0, 151 }, new []{151, 100}, new []{251, 135}, // { where the generation starts, how many pokemon in the generation }
        new []{386, 107}, new []{493, 156}, new []{649, 72}, 
        new []{721, 88}, new []{809, 89}, new []{897, 10_000} };  // last gen 9 is called 8+ meaning everything after gen 8

    public (int offset, int populationCount) GenerationRange(int gen) => gen > 0 && gen < _generations.Length ?
        ( _generations[gen][0], _generations[gen][1]) : (0, 151);

    public async Task<List<Pokemon>> GetAllPokemonAsync()
    {
        if (_pokemons.Count == 0)
        {
            await GetAllPokemonFromApiAsync();
        }

        return _pokemons;
    }


    public async Task<Pokemon> GetPokemonAsync(int id)
    {
        if (_pokemons.Count == 0)
        {
            await GetAllPokemonFromApiAsync();
        }

        return _pokemons.FirstOrDefault(p => p.Id == id) ?? new(id);
    }


    private async Task GetAllPokemonFromApiAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, _allSpeciesUrl);
        using var client = _clientFactory.CreateClient("Pokeapi");
        using var response = await client.SendAsync(request);

        _pokemons = await ExtractPokemonAsync(response);
    }

    private static async Task<List<Pokemon>> ExtractPokemonAsync(HttpResponseMessage response)
    {
        var result = new List<Pokemon>();
        var species = new List<SpeciesResponse>();
        
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
            }
        }

        foreach (var specimen in species)
        {
            result.Add(new Pokemon(GetIdFromUrl(specimen.url), specimen.name));
        }

        return result;
    }

    private static int GetIdFromUrl(string url)
    {
        int.TryParse(PokemonIdFromUrl.Match(url).Groups["id"].Value, out var id);
        return id;
    }

    private async void GetEvolutionsFor(int id)
    {
        using var client = _clientFactory.CreateClient("Pokeapi");
        using var request = new HttpRequestMessage(HttpMethod.Get, _allSpeciesUrl + id);
        using var response = await client.SendAsync(request);
        var detailResponse = new PokeapiSpeciesDetailResponse();

        if (response.IsSuccessStatusCode)
        {
            try
            {
                detailResponse = await response.Content.ReadFromJsonAsync<PokeapiSpeciesDetailResponse>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        var evolutionChainUrl = detailResponse?.EvolutionChain?.url ?? "";
        if (evolutionChainUrl == "")
            return;

        using var evolutionChainRequest = new HttpRequestMessage(HttpMethod.Get, evolutionChainUrl);
        using var pokeapiResponse = await client.SendAsync(evolutionChainRequest);
        EvolutionChain? chain = null;

        if (pokeapiResponse.IsSuccessStatusCode)
        {
            try
            {
               var evolutionChainResponse = await pokeapiResponse.Content.ReadFromJsonAsync<PokeapiEvolutionChainResponse>();
               chain = evolutionChainResponse?.Chain;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        if (chain is null)
            return;

        var evolutionChain = OrderByEvolution(chain);

    }

    private async Task<List<List<Pokemon>>> OrderByEvolution(EvolutionChain chain)
    {
        var result = new List<List<Pokemon>>();

        var currentPokemon = _pokemons.FirstOrDefault(p => p.Name == chain.Species.name) ?? new();
        result.Add(new List<Pokemon> { currentPokemon } );
        
        // need to make recursive data structure instead of list<list>
        
       


        return result;
    }

}