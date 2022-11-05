using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record NextAiringEpisode(
	[property: JsonPropertyName("episode")]
	int Episode
);