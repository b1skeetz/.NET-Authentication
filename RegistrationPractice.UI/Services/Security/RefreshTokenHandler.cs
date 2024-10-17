using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Blazored.LocalStorage;
using RegistrationPractice.WebApi.Contracts.Models;

namespace RegistrationPractice.UI.Services.Security;

public class RefreshTokenHandler(ILogger<RefreshTokenHandler> logger, ILocalStorageService localStorage)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Request: {@Request}", request))
        {
            var currentInfo = await localStorage.GetItemAsync<AuthenticationResponse>("user", cancellationToken);
            if (currentInfo != null)
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + currentInfo.Token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized || currentInfo == null) return response;

            var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/refresh-token");

            refreshRequest.Content =
                new StringContent(currentInfo.RefreshToken, Encoding.UTF8, "application/json");

            var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);
            var authInfo = await refreshResponse.Content.ReadFromJsonAsync<AuthenticationResponse>(cancellationToken);

            await localStorage.SetItemAsync("user", refreshResponse.Content, cancellationToken);

            logger.LogInformation("User: {@User}", authInfo.UserName);
            logger.LogInformation("JwtToken: {@User}", authInfo.Token);
            logger.LogInformation("RefreshToken: {@User}", authInfo.RefreshToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}