using App.Models.Auth.Shared.HttpHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App.Client.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddClientApplicationLayer();
builder.Services.AddMudServices();

builder.Services.AddScoped<HttpTokenAuthHeaderHandler>();
builder.Services.AddHttpClient(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<HttpTokenAuthHeaderHandler>();

await builder.Build().RunAsync();
