using Blazored.LocalStorage;

using PlanningListFilterer.Models.Anilist;

using System.Collections.Immutable;

namespace PlanningListFilterer.Settings;

public sealed record ColumnSettings(
	ImmutableHashSet<string> HiddenColumns
)
{
	public static ImmutableHashSet<string> DefaultHidden { get; } = new[]
	{
		nameof(AnilistModel.Episodes),
		nameof(AnilistModel.FriendScore),
		nameof(AnilistModel.FriendPopularityScored),
		nameof(AnilistModel.FriendPopularityTotal),
		nameof(AnilistModel.Format),
		nameof(AnilistModel.IsSequel),
	}.ToImmutableHashSet();
	public static ImmutableHashSet<string> FriendScores { get; } = new[]
	{
		nameof(AnilistModel.FriendScore),
		nameof(AnilistModel.FriendPopularityScored),
		nameof(AnilistModel.FriendPopularityTotal),
	}.ToImmutableHashSet();
	public static ImmutableHashSet<string> GlobalScores { get; } = new[]
	{
		nameof(AnilistModel.Popularity),
		nameof(AnilistModel.Score),
	}.ToImmutableHashSet();

	public ColumnSettings() : this(HiddenColumns: DefaultHidden)
	{
	}
}

public class ColumnSettingsService(ILocalStorageService localStorage)
	: LocalStorageJsonSettingsService<ColumnSettings>(localStorage)
{
	protected override string Key => "ColumnSettings";
}