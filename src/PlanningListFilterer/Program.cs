using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;

using PlanningListFilterer;
using PlanningListFilterer.Settings;

using System.Text.Json.Serialization;

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
builder.Services.AddScoped<SettingsService>();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage(config =>
{
	config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

await builder.Build().RunAsync().ConfigureAwait(false);