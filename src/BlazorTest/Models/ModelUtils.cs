namespace BlazorTest.Models;

public static class ModelUtils
{
	public const string NO_VALUE = "N/A";

	public static string DisplayDuration(this AnilistMedia media)
	{
		var duration = media.GetTotalDuration();
		if (!duration.HasValue)
		{
			return NO_VALUE;
		}
		return $"{duration} minute{(duration == 1 ? "" : "s")}";
	}

	public static string DisplayEpisodeCount(this AnilistMedia media)
	{
		var count = media.GetHighestEpisode();
		if (!count.HasValue)
		{
			return NO_VALUE;
		}
		return $"{count} episode{(count == 1 ? "" : "s")}";
	}

	public static string DisplayFormat(this AnilistMedia media)
		=> media.Format?.ToString() ?? NO_VALUE;

	public static string DisplayGenres(this AnilistMedia media, bool expanded)
		=> DisplayExpandable(media.Genres, expanded);

	public static string DisplayScore(this AnilistMedia media)
	{
		var score = media.AverageScore;
		if (!score.HasValue)
		{
			return NO_VALUE;
		}
		return $"{score}%";
	}

	public static string DisplayStart(this AnilistMedia media)
	{
		var year = media.StartDate?.Year;
		if (!year.HasValue)
		{
			return NO_VALUE;
		}

		var month = media.StartDate!.Month;
		var format = month.HasValue ? "yyyy MMM" : "yyyy";
		return media.StartDate!.Start!.Value.ToString(format);
	}

	public static string DisplayTag(this AnilistMediaTag tag)
	{
		var name = tag.Name;
		if (name is "Cute Girls Doing Cute Things")
		{
			name = "CGDCT";
		}
		return $"{name} ({tag.Rank}%)";
	}

	public static string DisplayTags(this AnilistMedia media, bool expanded)
	{
		var tags = media.Tags.Select(x => x.DisplayTag());
		return DisplayExpandable(tags, expanded);
	}

	public static int? GetHighestEpisode(this AnilistMedia media)
		=> media.Episodes ?? media.NextAiringEpisode?.Episode;

	public static int? GetTotalDuration(this AnilistMedia media)
		=> media.GetHighestEpisode() * media.Duration;

	public static string GetUrl(this AnilistMedia media)
		=> $"https://anilist.co/anime/{media.Id}/";

	private static string DisplayExpandable(this IEnumerable<string> items, bool expanded)
	{
		if (!items.Any())
		{
			return NO_VALUE;
		}

		if (expanded)
		{
			return string.Join(Environment.NewLine, items.Skip(1));
		}
		return items.First();
	}
}