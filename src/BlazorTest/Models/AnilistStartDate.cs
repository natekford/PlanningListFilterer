using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistStartDate(
	[property: JsonPropertyName("year")]
	int? Year,
	[property: JsonPropertyName("month")]
	int? Month
)
{
	public DateTime? Start
	{
		get
		{
			if (Year is not int year)
			{
				return null;
			}
			return new DateTime(year: year, month: Month ?? 12, day: 1);
		}
	}
}