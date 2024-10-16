﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refit;
using RegistrationPractice.WebApi.Contracts.Models;

namespace RegistrationPractice.UI.Services.Http;

public interface IBackendApi
{
    [Post("/login")]
    Task<ApiResponse<AuthenticationResponse>> GetAuthenticated(string? returnUrl,[Body] UserModel request, CancellationToken cancellationToken = default);
    
    [Get("/logout")]
    Task<string> Logout(CancellationToken cancellationToken = default);
    
    [Post("/register")]
    Task<ApiResponse<string>> Register([Body]UserModel request, CancellationToken cancellationToken = default);
    
    [Post("/refresh-token")]
    Task<ApiResponse<AuthenticationResponse>> RefreshToken([Body] RefreshTokenModel model, CancellationToken cancellationToken = default);
    
    [Post("/home")]
    Task<ApiResponse<string>> GetHomeAsync(CancellationToken cancellationToken = default);
}