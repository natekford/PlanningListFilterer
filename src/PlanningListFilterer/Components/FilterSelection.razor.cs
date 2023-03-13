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
	public ImmutableArray<string> Options { get; private set; } = ImmutableArray<string>.Empty;
	public HashSet<string> SelectedItems { get; private set; } = null!;
	[Parameter]
	public Func<AnilistModel, IEnumerable<string>> Selector { get; set; } = null!;

	public void Clear()
	{
		SelectedItems.Clear();
		MarkFilterAsDisabled();
		GridStateHasChanged();
	}

	public void OnCheckedChanged(bool value, string item)
	{
		if (value)
		{
			SelectedItems.Add(item);
		}
		else
		{
			SelectedItems.Remove(item);
		}

		if (SelectedItems.Count == 0)
		{
			MarkFilterAsDisabled();
		}
		else if (SelectedItems.Count == 1)
		{
			MarkFilterAsEnabled();
		}

		UpdateOptions();
		GridStateHasChanged();
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
		filterDefinition.FilterFunction = m => SelectedItems.All(i => Selector(m).Contains(i));
		_FilterDefinition = filterDefinition;

		UpdateOptions();
	}

	private void GridStateHasChanged()
		// StateHasChanged needs to be invoked
		// This method does the least while still doing that
		=> Column.DataGrid.ToggleFiltersMenu();

	private void MarkFilterAsDisabled()
		=> _FilterDefinition.Operator = null;

	private void MarkFilterAsEnabled()
		=> _FilterDefinition.Operator = "ignored";

	private void UpdateOptions()
	{
		Options = Column.DataGrid.FilteredItems
			.SelectMany(Selector)
			.Distinct()
			.OrderByDescending(SelectedItems.Contains)
			.ThenBy(x => x)
			.ToImmutableArray();
	}
}