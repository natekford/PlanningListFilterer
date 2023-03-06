using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistEntry(
	[property: JsonPropertyName("media")]
	AnilistMedia Media
);