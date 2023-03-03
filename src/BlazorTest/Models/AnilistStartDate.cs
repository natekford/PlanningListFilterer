using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistStartDate(
	[property: JsonPropertyName("year")]
	int? Year
);