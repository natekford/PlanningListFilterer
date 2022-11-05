using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistResponse(
	[property: JsonPropertyName("data")]
	Data Data
);

public sealed record Data(
	[property: JsonPropertyName("MediaListCollection")]
	MediaListCollection MediaListCollection
);

public sealed record MediaListCollection(
	[property: JsonPropertyName("lists")]
	EntryList[] Lists
);

public sealed record EntryList(
	[property: JsonPropertyName("name")]
	string Name,
	[property: JsonPropertyName("isCustomList")]
	bool IsCustomList,
	[property: JsonPropertyName("isCompletedList")]
	bool IsCompletedList,
	[property: JsonPropertyName("entries")]
	Entry[] Entries
);

public sealed record Entry(
	[property: JsonPropertyName("media")]
	Media Media
);