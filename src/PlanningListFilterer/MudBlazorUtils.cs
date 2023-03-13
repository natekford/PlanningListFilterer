using MudBlazor;

namespace PlanningListFilterer;

public static class MudBlazorUtils
{
	public static void ExternalStateHasChanged<T>(this MudDataGrid<T> grid)
		// StateHasChanged needs to be invoked
		// This method does the least while still doing that
		=> grid.ToggleFiltersMenu();
}