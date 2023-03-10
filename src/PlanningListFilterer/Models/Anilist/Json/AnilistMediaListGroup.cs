using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistMediaListGroup(
	[property: JsonPropertyName("entries")]
	AnilistMediaList[] Entries
);