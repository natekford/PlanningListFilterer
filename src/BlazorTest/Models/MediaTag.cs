using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record MediaTag(
	[property: JsonPropertyName("name")]
	string Name,
	[property: JsonPropertyName("rank")]
	int Rank
);