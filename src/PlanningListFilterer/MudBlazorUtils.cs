using MudBlazor;

using System.Collections.Immutable;

namespace PlanningListFilterer;

public static class MudBlazorUtils
{
	public static void ExternalStateHasChanged<T>(this MudDataGrid<T> grid)
		// StateHasChanged needs to be invoked
		// This method does the least while still doing that
		=> grid.ToggleFiltersMenu();

	public static ImmutableHashSet<string> GetHiddenColumns<T>(this MudDataGrid<T> grid)
	{
		return grid.RenderedColumns
			.Where(x => x.Hidden)
			.Select(x => x.PropertyName)
			.ToImmutableHashSet();
	}

	public static Task SetVisibilityAsync<T>(this Column<T> column, bool visible)
		=> visible ? column.ShowAsync() : column.HideAsync();
}