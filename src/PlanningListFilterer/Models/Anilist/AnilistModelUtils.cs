using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public static class AnilistModelUtils
{
	public const string NO_VALUE = "N/A";
	public static DateTime NoReleaseDate { get; } = new(year: 2100, month: 1, day: 1);

	public static string DisplayAverageScore(this AnilistModel model)
		=> model.AverageScore is int s ? $"{s}%" : NO_VALUE;

	public static string DisplayDuration(this AnilistModel model)
		=> model.Duration is int d ? d.ToString("n0") : NO_VALUE;

	public static string DisplayEpisodeCount(this AnilistModel model)
		=> model.Episodes is int e ? e.ToString("n0") : NO_VALUE;

	public static string DisplayFriendPopularityScored(this AnilistModel model)
		=> model.FriendPopularityScored.ToString("n0");

	public static string DisplayFriendPopularityTotal(this AnilistModel model)
		=> model.FriendPopularityTotal.ToString("n0");

	public static string DisplayFriendScore(this AnilistModel model)
		=> model.FriendScore is int s ? $"{s}%" : NO_VALUE;

	public static string DisplayGenres(this AnilistModel model)
		=> model.Genres.DisplayStrings();

	public static string DisplayPersonalScore(this AnilistModel model)
		=> model.PersonalScore is int s ? $"{s}%" : NO_VALUE;

	public static string DisplayPopularity(this AnilistModel model)
		=> model.Popularity.ToString("n0");

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
		var name = tag.Key switch
		{
			"Cute Girls Doing Cute Things" => "CGDCT",
			"Cute Boys Doing Cute Things" => "CBDCT",
			_ => tag.Key,
		};
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
		=> year is int y ? new(year: y, month: month ?? 12, day: 1) : NoReleaseDate;

	public static string GetUrl(this AnilistModel model)
		=> $"https://anilist.co/anime/{model.Id}/";

	private static string DisplayStrings(this IEnumerable<string> items)
		=> items.Any() ? string.Join(Environment.NewLine, items) : NO_VALUE;
}