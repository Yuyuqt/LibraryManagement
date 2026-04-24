using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorFrontend.Providers;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());
    
    public string? Token { get; private set; }

    public JwtAuthenticationStateProvider(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
            return Task.FromResult(new AuthenticationState(_anonymous));

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(Token);

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }
        catch
        {
            return Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task LoadTokenAsync()
    {
        try
        {
            var tokenResult = await _localStorage.GetAsync<string>("AuthToken");
            if (tokenResult.Success && !string.IsNullOrWhiteSpace(tokenResult.Value))
            {
                Token = tokenResult.Value;
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }
        catch
        {
            // Ignore JS interop errors if they somehow still happen
        }
    }

    public async Task LoginAsync(string token)
    {
        Token = token;
        await _localStorage.SetAsync("AuthToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        Token = null;
        await _localStorage.DeleteAsync("AuthToken");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
