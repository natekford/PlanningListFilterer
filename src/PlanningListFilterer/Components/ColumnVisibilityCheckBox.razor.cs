using Microsoft.AspNetCore.Components;

using MudBlazor;

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
		Grid.ExternalStateHasChanged();
	}

	public async Task EnableAll()
	{
		foreach (var column in Columns)
		{
			await column.ShowAsync().ConfigureAwait(false);
		}
		Grid.ExternalStateHasChanged();
	}

	public void OpenMenu()
		=> IsMenuOpen = true;

	public async Task SetVisible(Column<T> column, bool visible)
	{
		await (visible ? column.ShowAsync() : column.HideAsync()).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}
}