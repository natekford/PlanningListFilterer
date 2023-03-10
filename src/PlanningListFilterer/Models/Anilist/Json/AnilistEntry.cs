using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistEntry(
	[property: JsonPropertyName("media")]
	AnilistMedia Media
);