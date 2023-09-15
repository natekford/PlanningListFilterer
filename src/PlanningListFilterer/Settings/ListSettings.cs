using Blazored.LocalStorage;

namespace PlanningListFilterer.Settings;

public sealed class ListSettings
{
	public bool AutomaticallyToggleFriendScoreColumns { get; set; } = true;
	public bool AutomaticallyToggleGlobalScoreColumns { get; set; } = true;
	public bool EnableFriendScores { get; set; }
}

public class ListSettingsService(ILocalStorageService localStorage)
	: LocalStorageJsonSettingsService<ListSettings>(localStorage)
{
	protected override string Key => "ListSettings";
}