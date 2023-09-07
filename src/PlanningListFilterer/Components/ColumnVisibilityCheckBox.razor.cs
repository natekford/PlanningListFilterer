using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class ColumnVisibilityCheckBox<T>
{
	public List<Column<T>> Columns => Grid.RenderedColumns;
	[Parameter]
	public MudDataGrid<T> Grid { get; set; } = null!;
	public bool IsMenuOpen { get; set; }

	public void CloseMenu()
		=> IsMenuOpen = false;

	public async Task DisableAll()
	{
		foreach (var column in Columns)
		{
			await column.HideAsync().ConfigureAwait(false);
		}
		await Save().ConfigureAwait(false);
	}

	public async Task EnableAll()
	{
		foreach (var column in Columns)
		{
			await column.ShowAsync().ConfigureAwait(false);
		}
		await Save().ConfigureAwait(false);
	}

	public async Task OnCheckedChanged(Column<T> column, bool visible)
	{
		await (visible ? column.ShowAsync() : column.HideAsync()).ConfigureAwait(false);
		await Save().ConfigureAwait(false);
	}

	public void OpenMenu()
		=> IsMenuOpen = true;

	public async Task RestoreDefault()
	{
		var @default = new ColumnSettings();
		foreach (var column in Grid.RenderedColumns)
		{
			if (column.Hideable == false)
			{
				continue;
			}

			var visible = !@default.HiddenColumns.Contains(column.PropertyName);
			await (visible ? column.ShowAsync() : column.HideAsync()).ConfigureAwait(false);
		}
		await Save().ConfigureAwait(false);
	}

	public async Task Save()
	{
		var hiddenColumns = Grid.RenderedColumns
			.Where(x => x.Hidden)
			.Select(x => x.PropertyName)
			.ToHashSet();
		await ColumnSettingsService.SaveSettingsAsync(new(
			HiddenColumns: hiddenColumns
		)).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}
}