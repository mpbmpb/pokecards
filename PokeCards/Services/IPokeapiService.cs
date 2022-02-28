using PokeCards.Data;

namespace PokeCards.Services;

public interface IPokeapiService
{
    public Task<List<Pokemon>> GetAllPokemonAsync();
    public Task<Pokemon> GetPokemonAsync(int id);
    public (int offset, int populationCount) GenerationRange(int gen);
}