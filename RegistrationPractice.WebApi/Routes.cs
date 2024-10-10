using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistrationPractice.DataAccess;
using RegistrationPractice.Domain;
using RegistrationPractice.WebApi.Contracts.Models;

namespace RegistrationPractice.WebApi;

public static class Routes
{
    public static void MapRoutes(this WebApplication webApplication)
    {
        var apiGroup = webApplication.MapGroup("");

        apiGroup.MapPost("/login", async (string? returnUrl, 
                ApplicationDbContext context,  
                HttpContext httpContext, 
                [FromBody] UserModel request) =>
            {
                if (request.Email == "" || request.Password == "")
                {
                    return Results.BadRequest();
                }
                
                var currentUser = context.Users.FirstOrDefault(u => u.Email == request.Email 
                                                                    && u.Password == request.Password);
                if (currentUser is null)
                {
                    return Results.Unauthorized();
                }

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, currentUser.Email) };
                var claimsIdentity = new ClaimsIdentity(claims, "ApplicationCookie");
                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return Results.Redirect(returnUrl??"/");
            })
            .WithName("Login")
            .WithOpenApi();
        
        apiGroup.MapGet("/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        })
            .WithName("Logout")
            .WithOpenApi();

        apiGroup.MapGet("/", [Authorize]() => "Hello World!");
    }
}