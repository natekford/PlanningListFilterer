using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistListQuery(
	[property: JsonPropertyName("MediaListCollection")]
	AnilistMediaListCollection MediaListCollection
);