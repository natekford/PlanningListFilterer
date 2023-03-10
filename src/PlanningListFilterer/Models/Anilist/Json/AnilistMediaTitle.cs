using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaTitle(
	[property: JsonPropertyName("userPreferred")]
	string UserPreferred
);