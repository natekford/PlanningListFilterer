using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaListCollection(
	[property: JsonPropertyName("hasNextChunk")]
	bool HasNextChunk,
	[property: JsonPropertyName("user")]
	AnilistUser User,
	[property: JsonPropertyName("lists")]
	AnilistMediaListGroup[] Lists
);