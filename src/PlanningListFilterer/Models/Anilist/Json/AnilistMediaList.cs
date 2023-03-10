using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaList(
	[property: JsonPropertyName("media")]
	AnilistMedia Media,
	[property: JsonPropertyName("score")]
	double Score
);