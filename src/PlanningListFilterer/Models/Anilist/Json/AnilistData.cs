using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistData(
	[property: JsonPropertyName("MediaListCollection")]
	AnilistMediaListCollection MediaListCollection
);