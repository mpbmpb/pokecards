using System.Net;
using System.Net.Http;

namespace PokeCards.Tests.Helpers;

public static class MockHttpHelper
{
    public static HttpClient GetClient(string content, HttpStatusCode statusCode)
    {
        var messageHandler = new MockHttpMessageHandler(content, statusCode);
        return new HttpClient(messageHandler);
    }

    public static HttpClient GetClient(string[] content, HttpStatusCode statusCode)
    {
        var messageHandler = new MockHttpMessageHandler(content, statusCode);
        return new HttpClient(messageHandler);
    }

    public static HttpClient GetClient(string content)
        => GetClient(content, HttpStatusCode.OK);

    public static HttpClient GetClient(string[] content)
        => GetClient(content, HttpStatusCode.OK);

    public static IHttpClientFactory GetFactory(string content, HttpStatusCode statusCode)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        var client = GetClient(content, statusCode);
        factory.CreateClient(Arg.Any<string>()).Returns(client);

        return factory;
    }

   public static IHttpClientFactory GetFactory(string[] content, HttpStatusCode statusCode)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        var client = GetClient(content, statusCode);
        factory.CreateClient(Arg.Any<string>()).Returns(client);

        return factory;
    }

    public static IHttpClientFactory GetFactory(string content)
        => GetFactory(content, HttpStatusCode.OK);
    
    public static IHttpClientFactory GetFactory(string[] content)
        => GetFactory(content, HttpStatusCode.OK);
}