using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaRelations(
	[property: JsonPropertyName("edges")]
	IReadOnlyList<AnilistMediaEdge> Edges
);
