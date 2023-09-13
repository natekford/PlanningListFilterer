using Microsoft.AspNetCore.Components;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class ListSettingsMenu<T> : BaseGridMenu<T>
{
	public override string ButtonText => "Settings";
	[Parameter]
	public ListSettings Settings { get; set; } = null!;

	public async Task OnFriendScoresChanged(bool value)
	{
		Settings.EnableFriendScores = value;

		await UpdateColumnVisibility(value).ConfigureAwait(false);
		await Save().ConfigureAwait(false);
	}

	public async Task Save()
	{
		await ListSettingsService.SaveSettingsAsync(
			settings: Settings
		).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}

	private async Task UpdateColumnVisibility(bool visible)
	{
		foreach (var column in Columns)
		{
			if (!ColumnSettings.FriendScores.Contains(column.PropertyName))
			{
				continue;
			}

			await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		}

		await ColumnSettingsService.SaveSettingsAsync(new(
			HiddenColumns: Grid.GetHiddenColumns()
		)).ConfigureAwait(false);
	}
}