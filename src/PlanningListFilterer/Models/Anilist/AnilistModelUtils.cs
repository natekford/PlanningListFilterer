using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public static class AnilistModelUtils
{
	public const string NO_VALUE = "N/A";
	public static DateTime NoReleaseDate { get; } = new(year: 2100, month: 1, day: 1);

	public static string DisplayDuration(this AnilistModel model)
		=> model.Duration is int d ? d.ToString() : NO_VALUE;

	public static string DisplayEpisodeCount(this AnilistModel model)
		=> model.Episodes is int e ? e.ToString() : NO_VALUE;

	public static string DisplayFriendScore(this AnilistModel model)
		=> model.FriendScore is int s ? $"{s}%" : NO_VALUE;

	public static string DisplayGenres(this AnilistModel model)
		=> model.Genres.DisplayStrings();

	public static string DisplayScore(this AnilistModel model)
		=> model.Score is int s ? $"{s}%" : NO_VALUE;

	public static string DisplayStart(this AnilistModel model)
	{
		var start = model.Start;
		if (start == NoReleaseDate)
		{
			return NO_VALUE;
		}

		var format = model.Month.HasValue ? "yyyy MMM" : "yyyy";
		return start.ToString(format);
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

	public static DateTime GetDate(this IFuzzyDate date)
		=> GetDate(date.Year, date.Month);

	public static DateTime GetDate(int? year, int? month)
	{
		if (year is not int y)
		{
			return NoReleaseDate;
		}
		return new(year: y, month: month ?? 12, day: 1);
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