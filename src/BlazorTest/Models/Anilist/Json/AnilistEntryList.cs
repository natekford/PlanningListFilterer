using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistEntryList(
	[property: JsonPropertyName("name")]
	string Name,
	[property: JsonPropertyName("isCustomList")]
	bool IsCustomList,
	[property: JsonPropertyName("isCompletedList")]
	bool IsCompletedList,
	[property: JsonPropertyName("entries")]
	AnilistEntry[] Entries
);