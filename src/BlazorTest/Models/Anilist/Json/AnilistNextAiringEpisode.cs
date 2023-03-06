using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistNextAiringEpisode(
	[property: JsonPropertyName("episode")]
	int Episode
);