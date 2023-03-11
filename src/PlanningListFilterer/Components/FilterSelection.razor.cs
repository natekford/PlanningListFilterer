using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Models.Anilist;

using System.Collections.Immutable;

namespace PlanningListFilterer.Components;

public partial class FilterSelection
{
	private FilterDefinition<AnilistModel> _FilterDefinition = null!;

	[Parameter]
	public Column<AnilistModel> Column { get; set; } = null!;
	public ImmutableArray<string> Options => Column.DataGrid.FilteredItems
		.SelectMany(x => Property(x))
		.Distinct()
		.OrderByDescending(x => SelectedItems.Contains(x))
		.ThenBy(x => x)
		.ToImmutableArray();
	[Parameter]
	public Func<AnilistModel, IReadOnlyCollection<string>> Property { get; set; } = null!;
	public HashSet<string> SelectedItems { get; private set; } = null!;

	public void Clear()
	{
		SelectedItems.Clear();
		MarkFilterAsDisabled();
	}

	public void OnCheckedChanged(bool value, string item)
	{
		if (value)
		{
			SelectedItems.Add(item);
			MarkFilterAsEnabled();
		}
		else
		{
			SelectedItems.Remove(item);
			if (SelectedItems.Count == 0)
			{
				MarkFilterAsDisabled();
			}
		}

		// StateHasChanged needs to be invoked
		// This method does the least while still doing that
		Column.DataGrid.ToggleFiltersMenu();
	}

	protected override void OnParametersSet()
	{
		var filterDefinition = Column.DataGrid.FilterDefinitions
			.SingleOrDefault(x => x.Column == Column);
		if (filterDefinition is null)
		{
			filterDefinition = new()
			{
				Column = Column,
				Id = Guid.NewGuid(),
				Title = Column.Title,
				Value = new HashSet<string>(),
			};

			// Add it directly to the grid, going through
			// Column.FilterContext.Actions.ApplyFilter
			// causes some issue with the first click not opening the filter menu
			Column.DataGrid.AddFilter(filterDefinition);
		}

		SelectedItems = (HashSet<string>)filterDefinition.Value;
		filterDefinition.FilterFunction = m => SelectedItems.All(i => Property(m).Contains(i));
		_FilterDefinition = filterDefinition;
	}

	private void MarkFilterAsDisabled()
		=> _FilterDefinition.Operator = null;

	private void MarkFilterAsEnabled()
		=> _FilterDefinition.Operator = "ignored";
}