using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistEntryList(
	[property: JsonPropertyName("entries")]
	AnilistEntry[] Entries
);