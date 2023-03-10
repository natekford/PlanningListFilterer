using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaEdge(
	[property: JsonPropertyName("node")]
	AnilistMedia Node,
	[property: JsonPropertyName("relationType")]
	[property: JsonConverter(typeof(JsonStringEnumConverter))]
	AnilistMediaRelation RelationType
);