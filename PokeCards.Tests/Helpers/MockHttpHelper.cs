using System.Net;
using System.Net.Http;

namespace PokeCards.Tests.Helpers;

public class MockHttpHelper
{
    private MockHttpMessageHandler _messageHandler;

    public MockHttpHelper(string[] content, HttpStatusCode statusCode)
    {
        _messageHandler = new MockHttpMessageHandler(content, statusCode);
    }

    public MockHttpHelper(string[] content)
    {
        _messageHandler = new MockHttpMessageHandler(content, HttpStatusCode.OK);
    }

    public IHttpClientFactory GetFactory()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns( _ => new HttpClient(_messageHandler));

        return factory;
    }
}