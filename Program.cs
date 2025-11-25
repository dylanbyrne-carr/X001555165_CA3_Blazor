using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using F1RaceAnalytics;
using F1RaceAnalytics.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://api.openf1.org/v1/") });
builder.Services.AddScoped<OpenF1Service>();

await builder.Build().RunAsync();