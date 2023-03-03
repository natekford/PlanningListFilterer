using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistMediaListCollection(
	[property: JsonPropertyName("lists")]
	AnilistEntryList[] Lists
);
