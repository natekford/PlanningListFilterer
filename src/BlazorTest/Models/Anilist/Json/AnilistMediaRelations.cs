using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist.Json;

public sealed record AnilistMediaRelations(
	[property: JsonPropertyName("edges")]
	IReadOnlyList<AnilistMediaEdge> Edges
);
