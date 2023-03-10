using PlanningListFilterer;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;
using Blazored.LocalStorage;
using System.Text.Json.Serialization;
using PlanningListFilterer.Settings;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ =>
{
	return new HttpClient
	{
		BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
	};
});
builder.Services.AddScoped<ListSettingsService>();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage(config =>
{
	config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

await builder.Build().RunAsync().ConfigureAwait(false);