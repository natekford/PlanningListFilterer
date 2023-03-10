using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistPageInfo(
	[property: JsonPropertyName("hasNextPage")]
	bool HasNextPage
);