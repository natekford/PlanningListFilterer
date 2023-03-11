using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public static class AnilistModelUtils
{
	public const string NO_VALUE = "N/A";

	public static AnilistStartModel CreateStartModel(this AnilistMedia media)
	{
		return new(
			Year: media.StartDate?.Year,
			Month: media.StartDate?.Month
		);
	}

	public static string DisplayDuration(this AnilistModel model)
		=> model.TotalDuration is int d ? d.ToString() : NO_VALUE;

	public static string DisplayEpisodeCount(this AnilistModel model)
		=> model.Episodes is int e ? e.ToString() : NO_VALUE;

	public static string DisplayFormat(this AnilistModel model)
		=> model.Format?.ToString() ?? NO_VALUE;

	public static string DisplayFriendScore(this AnilistModel model)
		=> model.FriendScore is int s ? $"{s}%" : NO_VALUE;

	public static string DisplayGenres(this AnilistModel model)
		=> model.Genres.DisplayStrings();

	public static string DisplayScore(this AnilistModel model)
		=> model.Score is int s ? $"{s}%" : NO_VALUE;

	public static string DisplayStart(this AnilistModel model)
	{
		var start = model.Start;
		if (!start.Year.HasValue)
		{
			return NO_VALUE;
		}

		var format = start.Month.HasValue ? "yyyy MMM" : "yyyy";
		return start.Time!.Value.ToString(format);
	}

	public static string DisplayTag(this KeyValuePair<string, int> tag)
	{
		var name = tag.Key;
		if (name is "Cute Girls Doing Cute Things")
		{
			name = "CGDCT";
		}
		return $"{name} ({tag.Value}%)";
	}

	public static string DisplayTags(this IEnumerable<KeyValuePair<string, int>> tags)
		=> tags.Select(x => x.DisplayTag()).DisplayStrings();

	public static string DisplayTags(this AnilistModel model, int skip, int count)
	{
		return model.Tags
			.OrderByDescending(x => x.Value)
			.Skip(skip)
			.Take(count)
			.DisplayTags();
	}

	public static string GetUrl(this AnilistModel model)
		=> $"https://anilist.co/anime/{model.Id}/";

	private static string DisplayStrings(this IEnumerable<string> items)
	{
		if (!items.Any())
		{
			return NO_VALUE;
		}
		return string.Join(Environment.NewLine, items);
	}
}