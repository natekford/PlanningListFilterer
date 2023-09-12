using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace PlanningListFilterer.Components;

public abstract partial class BaseGridMenu<T>
{
	public abstract string ButtonText { get; }
	[Parameter]
	public RenderFragment ChildContent { get; set; } = null!;
	[CascadingParameter]
	public MudDataGrid<T> Grid { get; set; } = null!;
	public bool IsMenuOpen { get; set; }

	public void CloseMenu()
		=> IsMenuOpen = false;

	public void OpenMenu()
		=> IsMenuOpen = true;
}