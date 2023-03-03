using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistMediaTag(
	[property: JsonPropertyName("name")]
	string Name,
	[property: JsonPropertyName("rank")]
	int Rank
);