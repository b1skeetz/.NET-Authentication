using Microsoft.AspNetCore.Http;
using Refit;
using RegistrationPractice.WebApi.Contracts.Models;

namespace RegistrationPractice.UI.Services.Http;

public interface IBackendApi
{
    [Post("/login")]
    Task<IResult> GetAuthenticated(string? returnUrl, UserModel request, CancellationToken cancellationToken = default);
    
    [Get("/logout")]
    Task<IResult> Logout(CancellationToken cancellationToken = default);
}