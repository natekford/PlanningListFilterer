using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record QueryPlanningList(
	[property: JsonPropertyName("MediaListCollection")]
	AnilistMediaListCollection MediaListCollection
);