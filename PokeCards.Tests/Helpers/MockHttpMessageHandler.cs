using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PokeCards.Tests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string[] _responses;
    private readonly HttpStatusCode _statusCode;
    private int _requestCount = 0;

    public MockHttpMessageHandler(string response, HttpStatusCode statusCode)
    {
        _responses = new []{ response };
        _statusCode = statusCode;
    }
    
   public MockHttpMessageHandler(string[] responses, HttpStatusCode statusCode)
    {
        _responses = responses;
        _statusCode = statusCode;
    }
    

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var index = _requestCount;

        Interlocked.Increment(ref _requestCount);
        return new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_responses[index])
        };
    }
}