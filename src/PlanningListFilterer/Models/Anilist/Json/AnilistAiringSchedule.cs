using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistAiringSchedule(
	[property: JsonPropertyName("episode")]
	int Episode
);