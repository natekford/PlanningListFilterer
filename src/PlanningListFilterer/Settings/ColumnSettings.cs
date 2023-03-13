using Blazored.LocalStorage;

namespace PlanningListFilterer.Settings;

public sealed record ColumnSettings(
	HashSet<string> HiddenColumns
)
{
	public ColumnSettings() : this(
		HiddenColumns: new()
	)
	{
	}
}

public class ColumnSettingsService : LocalStorageJsonSettingsService<ColumnSettings>
{
	protected override string Key => "ColumnSettings";

	public ColumnSettingsService(ILocalStorageService localStorage) : base(localStorage)
	{
	}
}