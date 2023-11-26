using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class SettingsMenu<T>
{
	public List<Column<T>> Columns => Grid.RenderedColumns;
	[Parameter]
	public MudDataGrid<T> Grid { get; set; } = null!;
	public bool IsMenuOpen { get; set; }
	[Parameter]
	public ListSettings ListSettings { get; set; } = null!;

	public void CloseMenu()
		=> IsMenuOpen = false;

	public async Task ColumnsChanged(Column<T> column, bool visible)
	{
		await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ColumnsDisableAll()
	{
		foreach (var column in Columns)
		{
			await column.HideAsync().ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ColumnsEnableAll()
	{
		foreach (var column in Columns)
		{
			await column.ShowAsync().ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ColumnsRestoreDefault()
	{
		foreach (var column in Columns)
		{
			var visible = !ColumnSettings.DefaultHidden.Contains(column.PropertyName);
			await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ListFriendScoresChanged(bool value)
	{
		ListSettings.EnableFriendScores = value;
		if (ListSettings.AutomaticallyToggleFriendScoreColumns)
		{
			await Columns.SetVisibilityAsync(ColumnSettings.FriendScores, value).ConfigureAwait(false);
		}
		if (ListSettings.AutomaticallyToggleGlobalScoreColumns)
		{
			await Columns.SetVisibilityAsync(ColumnSettings.GlobalScores, !value).ConfigureAwait(false);
		}

		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public void OpenMenu()
		=> IsMenuOpen = true;

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
}