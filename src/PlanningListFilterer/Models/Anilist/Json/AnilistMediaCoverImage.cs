using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaCoverImage(
	[property: JsonPropertyName("medium")]
	string Medium
);