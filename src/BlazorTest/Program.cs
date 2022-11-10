using BlazorTest;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;

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
builder.Services.AddMudServices();

await builder.Build().RunAsync().ConfigureAwait(false);