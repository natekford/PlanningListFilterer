using Microsoft.AspNetCore.Components;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class ListSettingsMenu<T> : BaseGridMenu<T>
{
	public override string ButtonText => "Settings";
	[Parameter]
	public ListSettings Settings { get; set; } = null!;

	public async Task Save()
	{
		await ListSettingsService.SaveSettingsAsync(
			settings: Settings
		).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}
}