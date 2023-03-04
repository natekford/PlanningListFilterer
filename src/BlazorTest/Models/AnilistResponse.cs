using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistResponse(
	[property: JsonPropertyName("data")]
	AnilistData Data
);