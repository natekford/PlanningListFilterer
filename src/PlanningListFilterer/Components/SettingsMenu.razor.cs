using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class SettingsMenu<T>
{
	public List<Column<T>> Columns => Grid.RenderedColumns;
	[CascadingParameter]
	public MudDataGrid<T> Grid { get; set; } = null!;
	public bool IsMenuOpen { get; set; }
	[Parameter]
	public ListSettings ListSettings { get; set; } = null!;

	public void CloseMenu()
		=> IsMenuOpen = false;

	public async Task DisableAllColumns()
	{
		foreach (var column in Columns)
		{
			await column.HideAsync().ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task EnableAllColumns()
	{
		foreach (var column in Columns)
		{
			await column.ShowAsync().ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task OnColumnChanged(Column<T> column, bool visible)
	{
		await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task OnFriendScoresChanged(bool value)
	{
		ListSettings.EnableFriendScores = value;

		await UpdateFriendScoreColumnVisibility(value).ConfigureAwait(false);
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public void OpenMenu()
		=> IsMenuOpen = true;

	public async Task RestoreDefaultColumns()
	{
		var @default = new ColumnSettings();
		foreach (var column in Grid.RenderedColumns)
		{
			if (column.Hideable == false)
			{
				continue;
			}

			var visible = !@default.HiddenColumns.Contains(column.PropertyName);
			await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task SaveAndUpdateUI()
	{
		await ListSettingsService.SaveSettingsAsync(
			settings: ListSettings
		).ConfigureAwait(false);
		await ColumnSettingsService.SaveSettingsAsync(new(
			HiddenColumns: Grid.GetHiddenColumns()
		)).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}

	private async Task UpdateFriendScoreColumnVisibility(bool visible)
	{
		foreach (var column in Columns)
		{
			if (!ColumnSettings.FriendScores.Contains(column.PropertyName))
			{
				continue;
			}

			await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		}
	}
}