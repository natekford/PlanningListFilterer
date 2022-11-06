using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record Media(
	[property: JsonPropertyName("id")]
	int Id,
	[property: JsonPropertyName("title")]
	Title Title,
	[property: JsonPropertyName("format")]
	Format? Format,
	[property: JsonPropertyName("episodes")]
	int? Episodes,
	[property: JsonPropertyName("nextAiringEpisode")]
	NextAiringEpisode? NextAiringEpisode,
	[property: JsonPropertyName("duration")]
	int? Duration,
	[property: JsonPropertyName("averageScore")]
	int? AverageScore,
	[property: JsonPropertyName("popularity")]
	int Popularity,
	[property: JsonPropertyName("startDate")]
	StartDate? StartDate
);

public static class MediaUtils
{
	public static string DisplayDuration(this Media media)
	{
		var duration = media.GetTotalDuration() ?? 0;
		return $"{duration} minute{(duration == 1 ? "" : "s")}";
	}

	public static string DisplayEpisodeCount(this Media media)
	{
		var count = media.GetHighestEpisode() ?? 0;
		return $"{count} episode{(count == 1 ? "" : "s")}";
	}

	public static string DisplayFormat(this Media media)
		=> media.Format?.ToString() ?? "Unknown";

	public static string DisplayScore(this Media media)
	{
		var score = media.AverageScore ?? 0;
		return $"{score}%";
	}

	public static string DisplayYear(this Media media)
		=> (media.StartDate?.Year ?? 0).ToString();

	public static int? GetHighestEpisode(this Media media)
		=> media.Episodes ?? media.NextAiringEpisode?.Episode;

	public static int? GetTotalDuration(this Media media)
		=> media.GetHighestEpisode() * media.Duration;

	public static string GetUrl(this Media media)
		=> $"https://anilist.co/anime/{media.Id}/";
}