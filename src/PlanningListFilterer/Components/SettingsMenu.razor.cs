using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Interfaces;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class SettingsMenu<T>
{
	public List<Column<T>> Columns => Grid.RenderedColumns;
	[Parameter]
	public required ColumnSettings ColumnSettings { get; set; } = null!;
	[Parameter]
	public required MudDataGrid<T> Grid { get; set; } = null!;
	public bool IsMenuOpen { get; set; }
	[Parameter]
	public required ListSettings ListSettings { get; set; } = null!;
	[Inject]
	public required SettingsService Settings { get; set; } = null!;

	public void CloseMenu()
		=> IsMenuOpen = false;

	public async Task ColumnsChanged(Column<T> column, bool visible)
	{
		await ColumnSettings.SetVisibilityAsync(
			column: column,
			visible: visible
		).ConfigureAwait(false);
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ColumnsDisableAll()
		=> await ToggleColumns(_ => false).ConfigureAwait(false);

	public async Task ColumnsEnableAll()
		=> await ToggleColumns(_ => true).ConfigureAwait(false);

	public async Task ColumnsRestoreDefault()
		=> await ToggleColumns(x => !ColumnSettings.DefaultHidden.Contains(x)).ConfigureAwait(false);

	public async Task EnableFriendScoresChanged()
	{
		if (ListSettings.AutomaticallyToggleFriendScoreColumns)
		{
			await ColumnSettings.SetVisibilityAsync(
				columns: Columns,
				properties: ColumnSettings.FriendScores,
				visible: ListSettings.EnableFriendScores
			).ConfigureAwait(false);
		}
		if (ListSettings.AutomaticallyToggleGlobalScoreColumns)
		{
			await ColumnSettings.SetVisibilityAsync(
				columns: Columns,
				properties: ColumnSettings.GlobalScores,
				visible: !ListSettings.EnableFriendScores
			).ConfigureAwait(false);
		}

		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public void OpenMenu()
		=> IsMenuOpen = true;

	public async Task SaveAndUpdateUI()
	{
		await Settings.SaveAsync(settings: ListSettings).ConfigureAwait(false);
		await Settings.SaveAsync(settings: ColumnSettings).ConfigureAwait(false);
		((IMudStateHasChanged)Grid).StateHasChanged();
	}

	private async Task ToggleColumns(Func<string, bool> getVisibility)
	{
		foreach (var column in Columns)
		{
			await ColumnSettings.SetVisibilityAsync(
				column: column,
				visible: getVisibility.Invoke(column.PropertyName)
			).ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}
}