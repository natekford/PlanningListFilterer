using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistData(
	[property: JsonPropertyName("MediaListCollection")]
	AnilistMediaListCollection MediaListCollection
);