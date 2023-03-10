using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaTag(
	[property: JsonPropertyName("name")]
	string Name,
	[property: JsonPropertyName("rank")]
	int Rank
);