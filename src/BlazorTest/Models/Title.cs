using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record Title(
	[property: JsonPropertyName("userPreferred")]
	string UserPreferred
);