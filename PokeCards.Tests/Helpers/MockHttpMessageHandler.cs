using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PokeCards.Tests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _response;
    private readonly string _emptyResponse;
    private readonly HttpStatusCode _statusCode;
    private int _requestCount = 0;

    public MockHttpMessageHandler(string response, HttpStatusCode statusCode)
    {
        _response = response;
        _emptyResponse = "";
        _statusCode = statusCode;
    }
    
   public MockHttpMessageHandler(string response, string emptyResponse, HttpStatusCode statusCode)
    {
        _response = response;
        _emptyResponse = emptyResponse;
        _statusCode = statusCode;
    }
    

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = _response;
        if ( _requestCount > 0)
        {
            response = _emptyResponse;
        }

        Interlocked.Increment(ref _requestCount);
        return new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(response)
        };
    }
}