using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistTitle(
	[property: JsonPropertyName("userPreferred")]
	string UserPreferred
);