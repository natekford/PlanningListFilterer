using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistUser(
	[property: JsonPropertyName("id")]
	int Id
);