using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistEntry(
	[property: JsonPropertyName("media")]
	AnilistMedia Media
);