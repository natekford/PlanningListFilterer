using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistResponse<T>(
	[property: JsonPropertyName("data")]
	T Data
);