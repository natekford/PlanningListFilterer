using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistTitle(
	[property: JsonPropertyName("userPreferred")]
	string UserPreferred
);