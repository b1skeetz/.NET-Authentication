using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Refit;
using RegistrationPractice.UI;
using RegistrationPractice.UI.Services.Http;
using RegistrationPractice.UI.Services.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddTransient<RefreshTokenHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, MyAuthenticationStateProvider>();

#pragma warning disable IL2026
var backendApiEndpoint = builder.Configuration.GetValue<Uri>("Endpoints:Registration")!;
#pragma warning restore IL2026

builder.Services.AddRefitClient<IBackendApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = backendApiEndpoint;
    }).AddHttpMessageHandler<RefreshTokenHandler>();

builder.Services.AddMudServices().AddMudBlazorScrollManager();

await builder.Build().RunAsync();