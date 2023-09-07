using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class ListSettingsMenu<T>
{
	[Parameter]
	public MudDataGrid<T> Grid { get; set; } = null!;
	public bool IsMenuOpen { get; set; }
	[Parameter]
	public ListSettings Settings { get; set; } = null!;

	public void CloseMenu()
		=> IsMenuOpen = false;

	public void OpenMenu()
		=> IsMenuOpen = true;

	public async Task Save()
	{
		await ListSettingsService.SaveSettingsAsync(
			settings: Settings
		).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}
}