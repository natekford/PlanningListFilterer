using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class ColumnVisibilityMenu<T> : BaseGridMenu<T>
{
	public override string ButtonText => "Columns";
	public List<Column<T>> Columns => Grid.RenderedColumns;

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