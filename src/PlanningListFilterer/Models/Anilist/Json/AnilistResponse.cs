using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistResponse(
	[property: JsonPropertyName("data")]
	AnilistData Data
);