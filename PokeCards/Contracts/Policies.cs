using System.Net;
using Polly;
using Polly.CircuitBreaker;
using Polly.Contrib.WaitAndRetry;
using Polly.Wrap;

namespace PokeCards.Contracts;

public static class Policies
{
    public static AsyncPolicyWrap<HttpResponseMessage> PokemontcgPolicy
    {
        get
        {
            var fallBackPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
                    !r.IsSuccessStatusCode).Or<Exception>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5);
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(8);
            var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<Exception>(e => e is not BrokenCircuitException)
                .WaitAndRetryAsync(delay);
            var circuitBreaker = Policy.HandleResult<HttpResponseMessage>(r =>
                    !r.IsSuccessStatusCode).Or<Exception>()
                .AdvancedCircuitBreakerAsync(0.7, TimeSpan.FromSeconds(20), 12, TimeSpan.FromSeconds(30));

            var policy = fallBackPolicy.WrapAsync(retryPolicy).WrapAsync(timeoutPolicy).WrapAsync(circuitBreaker);
            return policy;
        }
    }
    
    public static AsyncPolicyWrap<HttpResponseMessage> PokeapiPolicy
    {
        get
        {
            var fallBackPolicy = Policy.HandleResult<HttpResponseMessage>(r =>
                    !r.IsSuccessStatusCode).Or<Exception>()
                .FallbackAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3);
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5);
            var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<Exception>(e => e is not BrokenCircuitException)
                .WaitAndRetryAsync(delay);
            var circuitBreaker = Policy.HandleResult<HttpResponseMessage>(r =>
                    !r.IsSuccessStatusCode).Or<Exception>()
                .AdvancedCircuitBreakerAsync(0.7, TimeSpan.FromSeconds(15), 4, TimeSpan.FromSeconds(20));

            var policy = fallBackPolicy.WrapAsync(retryPolicy).WrapAsync(timeoutPolicy).WrapAsync(circuitBreaker);
            return policy;
        }
    }
    
    public static AsyncPolicyWrap<HttpResponseMessage> ImageServicePolicy
    {
        get
        {
            Task<HttpResponseMessage> fallbackAction(DelegateResult<HttpResponseMessage> response, Context context, CancellationToken token)
            {
                if (response.Result?.StatusCode == HttpStatusCode.NotFound)
                {
                    var message = new HttpResponseMessage(){Content = response.Result.Content};
                    return Task.FromResult(message);         
                }
            
                var emptyMessage = new HttpResponseMessage() { Content = new StringContent("") };
                return Task.FromResult(emptyMessage);
            }

            Task onFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context)
            {
                // Console.WriteLine("Image fallback executed");
                return Task.CompletedTask;
            }

            var fallBackPolicy = Policy.HandleResult<HttpResponseMessage>(r
                => !r.IsSuccessStatusCode).Or<Exception>()
                .FallbackAsync( fallbackAction, onFallbackAsync);
            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), 2);
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(500));
            var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r 
                => r.StatusCode is not HttpStatusCode.NotFound and > HttpStatusCode.BadRequest)
                .Or<Exception>()
                .WaitAndRetryAsync(delay);

            var policy = fallBackPolicy.WrapAsync(retryPolicy).WrapAsync(timeoutPolicy);
            return policy;
        }
    }
    
}

