using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaConnection(
	[property: JsonPropertyName("edges")]
	IReadOnlyList<AnilistMediaEdge> Edges
);
