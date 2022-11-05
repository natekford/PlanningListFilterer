using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record StartDate(
	[property: JsonPropertyName("year")]
	int? Year
);