using Blazored.LocalStorage;

using PlanningListFilterer.Models.Anilist;

namespace PlanningListFilterer.Settings;

public sealed record ColumnSettings(
	HashSet<string> HiddenColumns
)
{
	public ColumnSettings() : this(
		HiddenColumns: new()
		{
			nameof(AnilistModel.Episodes),
			nameof(AnilistModel.FriendScore),
			nameof(AnilistModel.FriendPopularity),
			nameof(AnilistModel.Format),
			nameof(AnilistModel.IsSequel),
		}
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