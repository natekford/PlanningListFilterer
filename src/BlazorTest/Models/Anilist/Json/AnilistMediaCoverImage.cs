using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistMediaCoverImage(
	[property: JsonPropertyName("medium")]
	string Medium
);