using Microsoft.AspNetCore.Components;

using MudBlazor;

using System.Collections.Immutable;

namespace PlanningListFilterer.Components;

public partial class FilterSelection<T>
{
	private FilterDefinition<T> _FilterDefinition = null!;

	[Parameter]
	public Column<T> Column { get; set; } = null!;
	public ImmutableArray<string> Options { get; private set; } = ImmutableArray<string>.Empty;
	public HashSet<string> SelectedItems { get; private set; } = null!;
	[Parameter]
	public Func<T, IEnumerable<string>> Selector { get; set; } = null!;

	public void Clear()
	{
		SelectedItems.Clear();
		MarkFilterAsDisabled();
		Column.DataGrid.ExternalStateHasChanged();
	}

	public void OnCheckedChanged(string item, bool add)
	{
		if (add)
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
		else if (!IsFilterEnabled())
		{
			MarkFilterAsEnabled();
		}

		UpdateOptions();
		Column.DataGrid.ExternalStateHasChanged();
	}

	protected override async Task OnParametersSetAsync()
	{
		if (Column.DataGrid.FilterDefinitions
			.SingleOrDefault(x => x.Column == Column)
			is not FilterDefinition<T> filterDefinition)
		{
			filterDefinition = new FilterDefinition<T>()
			{
				Column = Column,
				Id = Guid.NewGuid(),
				Title = Column.Title,
				Value = new HashSet<string>(),
			};

			// Add it directly to the grid, going through
			// Column.FilterContext.Actions.ApplyFilter
			// causes some issue with the first click not opening the filter menu
			await Column.DataGrid.AddFilterAsync(filterDefinition).ConfigureAwait(false);
		}

		SelectedItems = (HashSet<string>)filterDefinition.Value!;
		filterDefinition.FilterFunction = m => SelectedItems.All(i => Selector(m).Contains(i));
		_FilterDefinition = filterDefinition;

		UpdateOptions();
	}

	private bool IsFilterEnabled()
		=> _FilterDefinition.Operator != null;

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