﻿using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Components;

public partial class ListSettingsMenu
{
	public bool IsMenuOpen { get; set; }
	[Parameter]
	public ListSettings Settings { get; set; } = null!;

	public void CloseMenu()
		=> IsMenuOpen = false;

	public void OpenMenu()
		=> IsMenuOpen = true;

	public async Task Save()
	{
		await ListSettingsService.SaveSettingsAsync(
			settings: Settings
		).ConfigureAwait(false);
	}
}