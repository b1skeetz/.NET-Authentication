using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using RegistrationPractice.DataAccess;
using RegistrationPractice.Domain;
using RegistrationPractice.WebApi.Contracts.Models;

namespace RegistrationPractice.WebApi;

public static class Routes
{
    [Inject] private static JSRuntime? JSRuntime { get; set; }

    public static void MapRoutes(this WebApplication webApplication)
    {
        var apiGroup = webApplication.MapGroup("");

        apiGroup.MapPost("/register", async (
                ApplicationDbContext context,
                [FromBody] UserModel request) =>
            {
                if (request.Email == "" || request.Password == "")
                {
                    return Results.BadRequest();
                }

                var currentUser = context.Users.FirstOrDefault(u => u.Email == request.Email
                                                                    && u.Password == request.Password);
                if (currentUser is not null)
                {
                    Results.Content($"Email {request.Email} is already in use.");
                    return Results.Problem(detail: $"Email {request.Email} is already in use.", statusCode: 500);
                }

                context.Users.Add(new User
                {
                    Email = request.Email,
                    Age = 1,
                    FirstName = "",
                    Password = request.Password,
                    Role = await context.Roles.FirstAsync(x => x.RoleName == "User"),
                    RefreshToken = string.Empty
                });
                await context.SaveChangesAsync();

                return Results.Ok();
            })
            .WithName("Register")
            .WithOpenApi();

        apiGroup.MapPost("/login", async (
                string? returnUrl,
                ApplicationDbContext context,
                [FromBody] UserModel request) =>
            {
                if (request.Email == "" || request.Password == "")
                {
                    return Results.BadRequest();
                }
                
                // InternalServerError

                var currentUser = context.Users.FirstOrDefault(u => u.Email == request.Email
                                                                    && u.Password == request.Password);
                if (currentUser is null)
                {
                    return Results.Unauthorized();
                }

                // формируем ответ
                var response = new AuthenticationResponse
                {
                    UserName = request.Email,
                    Token = GenerateJwtToken(currentUser.Email),
                    RefreshToken = GenerateRefreshToken()
                };

                currentUser.RefreshToken = response.RefreshToken;
                currentUser.RefreshTokenExpiration = DateTimeOffset.UtcNow.AddHours(12);
                await context.SaveChangesAsync();

                //await JSRuntime!.InvokeVoidAsync("localStorage.setItem", "user", response);
                return Results.Json(response);
            })
            .WithName("Login")
            .WithOpenApi();
        
        apiGroup.MapGet("/logout", async () =>
            {
                await JSRuntime!.InvokeVoidAsync("localStorage.removeItem", "user");
                return Results.Redirect("/login");
            })
            .WithName("Logout")
            .WithOpenApi();

        apiGroup.MapPost("/home", () => "Hello World!").RequireAuthorization();
    }
    
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];

        using (var numberGenerator = RandomNumberGenerator.Create())
        {
            numberGenerator.GetBytes(randomNumber);
        }

        return Convert.ToBase64String(randomNumber);
    }

    private static string GenerateJwtToken(string email)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, email) };
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.Issuer,
            audience: AuthOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}

public class AuthOptions
{
    public const string Issuer = "https://localhost:7270"; // издатель токена
    public const string Audience = "https://localhost:7229"; // потребитель токена
    private const string Key = "mysupersecret_secretsecretsecretkey!123"; // ключ для шифрации
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(Key));
}