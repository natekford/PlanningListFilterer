using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class ColumnVisibilityMenu<T> : BaseGridMenu<T>
{
	public override string ButtonText => "Columns";

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
		await column.SetVisibilityAsync(visible).ConfigureAwait(false);
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
			await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		}
		await Save().ConfigureAwait(false);
	}

	public async Task Save()
	{
		await ColumnSettingsService.SaveSettingsAsync(new(
			HiddenColumns: Grid.GetHiddenColumns()
		)).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}
}