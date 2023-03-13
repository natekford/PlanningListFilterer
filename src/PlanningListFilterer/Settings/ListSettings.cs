using Blazored.LocalStorage;

namespace PlanningListFilterer.Settings;

public sealed record ListSettings(
	bool EnableFriendScores
)
{
	public ListSettings() : this(
		EnableFriendScores: false
	)
	{
	}
}

public class ListSettingsService : LocalStorageJsonSettingsService<ListSettings>
{
	protected override string Key => "ListSettings";

	public ListSettingsService(ILocalStorageService localStorage) : base(localStorage)
	{
	}
}