using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistMedia(
	[property: JsonPropertyName("id")]
	int Id,
	[property: JsonPropertyName("title")]
	AnilistTitle Title,
	[property: JsonPropertyName("status")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AnilistMediaStatus? Status,
	[property: JsonPropertyName("format")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AnilistMediaFormat? Format,
	[property: JsonPropertyName("episodes")]
	int? Episodes,
	[property: JsonPropertyName("nextAiringEpisode")]
	AnilistNextAiringEpisode? NextAiringEpisode,
	[property: JsonPropertyName("duration")]
	int? Duration,
	[property: JsonPropertyName("averageScore")]
	int? AverageScore,
	[property: JsonPropertyName("popularity")]
	int Popularity,
	[property: JsonPropertyName("startDate")]
	AnilistStartDate? StartDate,
	[property: JsonPropertyName("coverImage")]
	AnilistMediaCoverImage? CoverImage,
	[property: JsonPropertyName("genres")]
	IReadOnlyList<string> Genres,
	[property: JsonPropertyName("tags")]
	IReadOnlyList<AnilistMediaTag> Tags
);