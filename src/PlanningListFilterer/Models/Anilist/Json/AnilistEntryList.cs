using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistEntryList(
	[property: JsonPropertyName("entries")]
	AnilistEntry[] Entries
);