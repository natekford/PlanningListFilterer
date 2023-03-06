using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistMediaListCollection(
	[property: JsonPropertyName("lists")]
	AnilistEntryList[] Lists
);