using Blazored.LocalStorage;

namespace PlanningListFilterer.Settings;

public sealed class ListSettings
{
	public bool EnableFriendScores { get; set; }
}

public class ListSettingsService : LocalStorageJsonSettingsService<ListSettings>
{
	protected override string Key => "ListSettings";

	public ListSettingsService(ILocalStorageService localStorage) : base(localStorage)
	{
	}
}