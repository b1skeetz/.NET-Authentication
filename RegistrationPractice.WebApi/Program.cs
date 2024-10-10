using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using RegistrationPractice.DataAccess;
using RegistrationPractice.WebApi;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorization();

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

app.MapRoutes();

app.Run();