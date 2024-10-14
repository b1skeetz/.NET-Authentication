using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using RegistrationPractice.WebApi.Contracts.Models;

namespace RegistrationPractice.UI.Services.Security;

public class MyAuthenticationStateProvider(ILocalStorageService localStorage)
    : AuthenticationStateProvider
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var userStorage = await localStorage.GetItemAsync<string>("user");
        if (userStorage == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        var token = JsonSerializer.Deserialize<AuthenticationResponse>(userStorage);

        if (string.IsNullOrEmpty(token.Token) || IsTokenExpired(token.Token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token.Token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    private bool IsTokenExpired(string token)
    {
        var jwtToken = _tokenHandler.ReadToken(token) as JwtSecurityToken;
        return jwtToken.ValidTo < DateTime.UtcNow;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var jwtToken = _tokenHandler.ReadToken(jwt) as JwtSecurityToken;
        return jwtToken?.Claims ?? [];
    }
}
