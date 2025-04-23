﻿using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Interfaces;

using System.Collections.Immutable;

namespace PlanningListFilterer.Components;

public partial class FilterSelection<T>
{
	private FilterDefinition<T> _FilterDefinition = null!;

	[Parameter]
	public required Column<T> Column { get; set; } = null!;
	public MudDataGrid<T> Grid => Column.DataGrid;
	public ImmutableArray<string> Options { get; set; } = [];
	public string Search { get; set; } = "";
	public HashSet<string> SelectedItems => (HashSet<string>)_FilterDefinition.Value!;
	[Parameter]
	public required Func<T, IEnumerable<string>> Selector { get; set; } = null!;

	public void Clear()
	{
		SelectedItems.Clear();
		Search = "";

		UpdateOptions(redraw: true);
	}

	public bool IsFilterEnabled()
		=> _FilterDefinition.Operator != null;

	public void MarkFilterAsDisabled()
		=> _FilterDefinition.Operator = null;

	public void MarkFilterAsEnabled()
		=> _FilterDefinition.Operator = "ignored";

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

		UpdateOptions(redraw: true);
	}

	public void UpdateOptions(bool redraw)
	{
		if (SelectedItems.Count == 0)
		{
			MarkFilterAsDisabled();
		}
		else if (!IsFilterEnabled())
		{
			MarkFilterAsEnabled();
		}

		Options =
		[
			.. Grid.FilteredItems
				.SelectMany(Selector)
				.Where(x =>
				{
					return string.IsNullOrWhiteSpace(Search)
						|| x.Contains(Search, StringComparison.OrdinalIgnoreCase)
						|| SelectedItems.Contains(x);
				})
				.Distinct()
				.OrderByDescending(SelectedItems.Contains)
				.ThenBy(x => x)
		];

		if (redraw)
		{
			((IMudStateHasChanged)Grid).StateHasChanged();
		}
	}

	protected override async Task OnParametersSetAsync()
	{
		if (Grid.FilterDefinitions.SingleOrDefault(x => x.Column == Column)
			is not FilterDefinition<T> filterDefinition)
		{
			filterDefinition = new FilterDefinition<T>()
			{
				Column = Column,
				Id = Guid.NewGuid(),
				Title = Column.Title,
				Value = new HashSet<string>(),
				FilterFunction = m => SelectedItems.All(i => Selector(m).Contains(i)),
			};

			// Add it directly to the grid, going through
			// Column.FilterContext.Actions.ApplyFilter
			// causes some issue with the first click not opening the filter menu
			await Grid.AddFilterAsync(filterDefinition).ConfigureAwait(false);
		}
		_FilterDefinition = filterDefinition;

		UpdateOptions(redraw: false);
	}
}