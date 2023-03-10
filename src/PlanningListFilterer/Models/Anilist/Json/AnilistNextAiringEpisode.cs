using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistNextAiringEpisode(
	[property: JsonPropertyName("episode")]
	int Episode
);