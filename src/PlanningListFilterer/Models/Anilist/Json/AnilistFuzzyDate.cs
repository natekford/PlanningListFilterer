using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistFuzzyDate(
	[property: JsonPropertyName("year")]
	int? Year,
	[property: JsonPropertyName("month")]
	int? Month
) : IFuzzyDate;