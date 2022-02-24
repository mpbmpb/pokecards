using System.Collections.Generic;
using System.Text.Json;
using AutoMapper;
using Bogus;
using PokeCards.Contracts.Responses;
using PokeCards.Data;

namespace PokeCards.Tests.Helpers;

public static class DataHelper
{
    public static Faker<PokemontcgCardResponse> GetCardResponseFaker(int[] pokedexNumbers)
    {
        var faker = new Faker<PokemontcgCardResponse>()
            .RuleFor(x => x.Id, f => f.IndexFaker.ToString())
            .RuleFor(x => x.Name, f => f.Person.FirstName)
            .RuleFor(x => x.Supertype, f => f.Random.Word())
            .RuleFor(x => x.Types, f => f.Lorem.Words(3))
            .RuleFor(x => x.PokedexNumbers, pokedexNumbers);

        return faker;
    }
    
    public static Faker<PokemontcgCardResponse> GetCardResponseFaker(int pokedexNumber) 
        => GetCardResponseFaker(new[] { pokedexNumber });

    public static Faker<PokemontcgCardResponse> GetCardResponseFaker()
    {
        var rnd = new Random();
        return GetCardResponseFaker(rnd.Next(1, 800));
    }

    public static Faker<PokemontcgResponse> GetPokemontcgResponseFaker(int[] pokedexNumbers, int count, int pagesize, int page, int totalCount)
    {
        var cards = GetCardResponseFaker(pokedexNumbers).Generate(count).ToArray();

        var faker = new Faker<PokemontcgResponse>()
            .RuleFor(x => x.Data, f => cards)
            .RuleFor(x => x.Page, f => page)
            .RuleFor(x => x.PageSize, f => pagesize)
            .RuleFor(x => x.Count, f => count)
            .RuleFor(x => x.TotalCount, f => totalCount);

        return faker;
    }
    
    public static Faker<PokemontcgResponse> GetPokemontcgResponseFaker(int[] pokedexNumbers, int count, int pagesize, int page)
    {
        var cards = GetCardResponseFaker(pokedexNumbers).Generate(count).ToArray();

        var faker = new Faker<PokemontcgResponse>()
            .RuleFor(x => x.Data, f => cards)
            .RuleFor(x => x.Page, f => page)
            .RuleFor(x => x.PageSize, f => pagesize)
            .RuleFor(x => x.Count, f => count)
            .RuleFor(x => x.TotalCount, f => count);

        return faker;
    }

    public static Faker<PokemontcgResponse> GetPokemontcgResponseFaker(int pokedexNumber, int count, int pagesize, int page, int totalCount)
        => GetPokemontcgResponseFaker(new[] { pokedexNumber }, count, pagesize, page, totalCount);

    public static Faker<PokemontcgResponse> GetPokemontcgResponseFaker(int pokedexNumber, int count, int pagesize, int page)
        => GetPokemontcgResponseFaker(new[] { pokedexNumber }, count, pagesize, page);

    public static Faker<PokemontcgResponse> GetPokemontcgResponseFaker(int count, int pagesize, int page)
        => GetPokemontcgResponseFaker(1, count, pagesize, page);
    
    public static string GetPokemontcgResponseJson(int numberOfCards, int pageSize)
    {
        var response = GetPokemontcgResponseFaker(numberOfCards, pageSize, 1).Generate();

        return JsonSerializer.Serialize(response);
    }
    
    public static string GetEmptyPokemontcgResponseJson(int numberOfCards, int pageSize, int page)
    {
        var response = GetPokemontcgResponseFaker(1,0, pageSize, page, numberOfCards).Generate();

        return JsonSerializer.Serialize(response);
    }

    public static string[] GetPokemontcgResponsesJson(int numberOfCards, int pageSize, int numberOfPages)
    {
        var responses = new string[numberOfPages];
        responses[0] = GetPokemontcgResponseJson(numberOfCards, pageSize);

        for (int i = 1; i < numberOfPages; i++)
        {
            responses[i] = GetEmptyPokemontcgResponseJson(numberOfCards, pageSize, i + 1);
        }

        return responses;
    }

    public static Faker<PokemontcgResponse>[] GetPokemontcgResponseFakers(int numberOfCards, int pageSize, int numberOfPages)
    {
        var fakers = new Faker<PokemontcgResponse>[numberOfPages];
        fakers[0] = GetPokemontcgResponseFaker(numberOfCards, pageSize, 1);

        for (int i = 1; i < numberOfPages; i++)
        {
            fakers[i] = GetPokemontcgResponseFaker(numberOfCards, pageSize, i + 1);
        }

        return fakers;
    }
    
    public static IEnumerable<Card> ExtractCards(string[] responses)
    {
        var pokemontcgResponse = JsonSerializer.Deserialize<PokemontcgResponse>(responses[0]);
        var config = new MapperConfiguration(cfg => cfg.CreateMap<PokemontcgCardResponse, Card>());
        var mapper = config.CreateMapper();
        return pokemontcgResponse.Data.Select(r => mapper.Map<Card>(r));
    }
}