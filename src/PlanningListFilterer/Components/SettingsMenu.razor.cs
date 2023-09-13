﻿using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class SettingsMenu<T>
{
	public List<Column<T>> Columns => Grid.RenderedColumns;
	[Parameter]
	public MudDataGrid<T> Grid { get; set; } = null!;
	public bool IsMenuOpen { get; set; }
	[Parameter]
	public ListSettings ListSettings { get; set; } = null!;

	public void CloseMenu()
		=> IsMenuOpen = false;

	public async Task ColumnsChanged(Column<T> column, bool visible)
	{
		await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ColumnsDisableAll()
	{
		foreach (var column in Columns)
		{
			await column.HideAsync().ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ColumnsEnableAll()
	{
		foreach (var column in Columns)
		{
			await column.ShowAsync().ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ColumnsRestoreDefault()
	{
		var @default = new ColumnSettings();
		foreach (var column in Columns)
		{
			var visible = !@default.HiddenColumns.Contains(column.PropertyName);
			await column.SetVisibilityAsync(visible).ConfigureAwait(false);
		}
		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public async Task ListFriendScoresChanged(bool value)
	{
		ListSettings.EnableFriendScores = value;
		foreach (var column in Columns)
		{
			if (!ColumnSettings.FriendScores.Contains(column.PropertyName))
			{
				continue;
			}

			await column.SetVisibilityAsync(value).ConfigureAwait(false);
		}

		await SaveAndUpdateUI().ConfigureAwait(false);
	}

	public void OpenMenu()
		=> IsMenuOpen = true;

	private async Task SaveAndUpdateUI()
	{
		await ListSettingsService.SaveSettingsAsync(
			settings: ListSettings
		).ConfigureAwait(false);
		await ColumnSettingsService.SaveSettingsAsync(new(
			HiddenColumns: Grid.GetHiddenColumns()
		)).ConfigureAwait(false);
		Grid.ExternalStateHasChanged();
	}
}