using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistTitle(
	[property: JsonPropertyName("userPreferred")]
	string UserPreferred
);