using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistData(
	[property: JsonPropertyName("MediaListCollection")]
	AnilistMediaListCollection MediaListCollection
);