using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refit;
using RegistrationPractice.WebApi.Contracts.Models;

namespace RegistrationPractice.UI.Services.Http;

public interface IBackendApi
{
    [Post("/login")]
    Task<AuthenticationResponse> GetAuthenticated(string? returnUrl, UserModel request, CancellationToken cancellationToken = default);
    
    [Get("/logout")]
    Task<string> Logout(CancellationToken cancellationToken = default);
    
    
}