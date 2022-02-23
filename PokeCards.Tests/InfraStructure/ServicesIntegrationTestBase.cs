using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using PokeCards.Contracts;
using PokeCards.Services;

namespace PokeCards.Tests.InfraStructure;

public class ServicesIntegrationTestBase : IDisposable
{
    public readonly PokeapiService PokeapiService;
    public readonly PokemontcgService PokemontcgService;

    public ServicesIntegrationTestBase()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddHttpClient("Pokemontcg").AddPolicyHandler(Policies.PokemontcgPolicy);
        services.AddHttpClient("Pokeapi").AddPolicyHandler(Policies.PokeapiPolicy);
        services.AddHttpClient("ImageService").AddPolicyHandler(Policies.ImageServicePolicy);

        var clientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        PokeapiService = new PokeapiService(clientFactory);
        PokemontcgService = new(clientFactory, PokeapiService);
    }

    public void Dispose()
    {
        GC.Collect();
    }
}