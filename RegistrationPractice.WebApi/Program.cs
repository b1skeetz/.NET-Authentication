using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using Npgsql;
using RegistrationPractice.DataAccess;
using RegistrationPractice.UI.Services.Security;
using RegistrationPractice.WebApi;
using RegistrationPractice.WebApi.Contracts.Models;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddSingleton<IJSRuntime, JSRuntime>(); // ?
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("Database"));
var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(dataSource);
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // указывает, будет ли валидироваться издатель при валидации токена
            ValidateIssuer = true,
            // строка, представляющая издателя
            ValidIssuer = AuthOptions.Issuer,
            // будет ли валидироваться потребитель токена
            ValidateAudience = true,
            // установка потребителя токена
            ValidAudience = AuthOptions.Audience,
            // будет ли валидироваться время существования
            ValidateLifetime = true,
            // установка ключа безопасности
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            // валидация ключа безопасности
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("LocalEnv", b => b
        .WithOrigins("https://localhost:7229")
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("LocalEnv");
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger/index.html")).ExcludeFromDescription();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapPost("/refresh-token", async (
        ApplicationDbContext context,
        [FromBody] RefreshTokenModel model) =>
    {
        var principal = GetPrincipalFromToken(model.JwtToken!);
        if (principal?.Identity.Name is null)
        {
            return Results.Problem(detail: "Invalid token", statusCode: 500);
        }
                
        var currentUser = await context.Users.FirstAsync(x => x.Email == principal.Identity.Name);

        if (!currentUser.RefreshToken.Equals(model.RefreshToken) ||
            currentUser.RefreshTokenExpiration > DateTimeOffset.UtcNow)
        {
            return Results.Redirect("/login");
        }
                
        // формируем ответ
        var response = new AuthenticationResponse
        {
            UserName = currentUser.Email,
            Token = GenerateJwtToken(currentUser.Email),
            RefreshToken = GenerateRefreshToken()
        };

        currentUser.RefreshToken = response.RefreshToken;
        currentUser.RefreshTokenExpiration = DateTimeOffset.UtcNow.AddHours(12);
        await context.SaveChangesAsync();
        
        return Results.Json(response);
        
        ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            var securityKey = AuthOptions.GetSymmetricSecurityKey();

            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = false
            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
        } 
        
        string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }
        string GenerateJwtToken(string email)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, email) };
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.Issuer,
                audience: AuthOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    })
    .WithName("RefreshToken")
    .WithOpenApi()
    .RequireAuthorization();

app.MapRoutes();

app.Run();

