using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using BlazorFrontend.Providers;

namespace BlazorFrontend.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthHeaderHandler(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("=== HANDLER RUN ===");
        
        try
        {
            var jwtProvider = _authStateProvider as JwtAuthenticationStateProvider;
            var token = jwtProvider?.Token;

            if (!string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("TOKEN FOUND: " + token);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                Console.WriteLine("NO TOKEN FOUND");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
