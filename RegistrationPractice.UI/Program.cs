using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Refit;
using RegistrationPractice.UI;
using RegistrationPractice.UI.Services.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#pragma warning disable IL2026
var backendApiEndpoint = builder.Configuration.GetValue<Uri>("Endpoints:Registration")!;
#pragma warning restore IL2026

builder.Services.AddRefitClient<IBackendApi>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = backendApiEndpoint;
    });

builder.Services.AddMudServices().AddMudBlazorScrollManager();

await builder.Build().RunAsync();