using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistNextAiringEpisode(
	[property: JsonPropertyName("episode")]
	int Episode
);