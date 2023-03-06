using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistResponse(
	[property: JsonPropertyName("data")]
	AnilistData Data
);