using MudBlazor;

using PlanningListFilterer.Models.Anilist;

using System.Collections.Immutable;

namespace PlanningListFilterer.Settings;

public sealed class ColumnSettings
{
	public static ImmutableHashSet<string> DefaultHidden { get; } =
	[
		nameof(AnilistModel.Episodes),
		nameof(AnilistModel.Format),
		nameof(AnilistModel.FriendScore),
		nameof(AnilistModel.FriendPopularityScored),
		nameof(AnilistModel.FriendPopularityTotal),
		nameof(AnilistModel.IsSequel),
		nameof(AnilistModel.PersonalScore),
		nameof(AnilistModel.ScoreDiffAverage),
		nameof(AnilistModel.ScoreDiffFriends),
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
		nameof(AnilistModel.AverageScore),
	];

	public HashSet<string> HiddenColumns { get; set; } = new(DefaultHidden);

	public Task SetVisibilityAsync<T>(Column<T> column, bool visible)
	{
		if (visible)
		{
			HiddenColumns.Remove(column.PropertyName);
			return column.ShowAsync();
		}
		else
		{
			HiddenColumns.Add(column.PropertyName);
			return column.HideAsync();
		}
	}

	public async Task SetVisibilityAsync<T>(
		IEnumerable<Column<T>> columns,
		ICollection<string> properties,
		bool visible)
	{
		foreach (var column in columns)
		{
			if (column.Hideable != false && properties.Contains(column.PropertyName))
			{
				await SetVisibilityAsync(column, visible).ConfigureAwait(false);
			}
		}
	}
}