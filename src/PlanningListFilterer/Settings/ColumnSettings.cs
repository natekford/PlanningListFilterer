using Blazored.LocalStorage;

using PlanningListFilterer.Models.Anilist;

using System.Collections.Immutable;

namespace PlanningListFilterer.Settings;

public sealed record ColumnSettings(
	ImmutableHashSet<string> HiddenColumns
)
{
	public static ImmutableHashSet<string> DefaultHidden { get; } =
	[
		nameof(AnilistModel.Episodes),
		nameof(AnilistModel.FriendScore),
		nameof(AnilistModel.FriendPopularityScored),
		nameof(AnilistModel.FriendPopularityTotal),
		nameof(AnilistModel.Format),
		nameof(AnilistModel.IsSequel),
	];
	public static ImmutableHashSet<string> FriendScores { get; } =
	[
		nameof(AnilistModel.FriendScore),
		nameof(AnilistModel.FriendPopularityScored),
		nameof(AnilistModel.FriendPopularityTotal),
	];
	public static ImmutableHashSet<string> GlobalScores { get; } =
	[
		nameof(AnilistModel.Popularity),
		nameof(AnilistModel.Score),
	];

	public ColumnSettings() : this(HiddenColumns: DefaultHidden)
	{
	}
}

public class ColumnSettingsService(ILocalStorageService localStorage)
	: LocalStorageJsonSettingsService<ColumnSettings>(localStorage)
{
	protected override string Key => "_ColumnSettings";
}