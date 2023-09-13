﻿using Blazored.LocalStorage;

namespace PlanningListFilterer.Settings;

public sealed class ListSettings
{
	public bool EnableFriendScores { get; set; }
}

public class ListSettingsService(ILocalStorageService localStorage)
	: LocalStorageJsonSettingsService<ListSettings>(localStorage)
{
	protected override string Key => "ListSettings";
}