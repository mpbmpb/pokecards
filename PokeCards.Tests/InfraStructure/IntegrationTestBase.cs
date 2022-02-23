using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using PokeCards.Contracts;

namespace PokeCards.Tests.InfraStructure;

public class IntegrationTestBase
{
    protected readonly IHttpClientFactory ClientFactory;

    public IntegrationTestBase()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddHttpClient("Pokemontcg").AddPolicyHandler(Policies.PokemontcgPolicy);
        services.AddHttpClient("Pokeapi").AddPolicyHandler(Policies.PokeapiPolicy);
        services.AddHttpClient("ImageService").AddPolicyHandler(Policies.ImageServicePolicy);

        ClientFactory= services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
    }
}