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
)
{
	public string Url => $"https://anilist.co/anime/{Id}/";
	public int? HighestAvailableEpisode => Episodes ?? NextAiringEpisode?.Episode;
	public int? TotalDuration => HighestAvailableEpisode * Duration;
}