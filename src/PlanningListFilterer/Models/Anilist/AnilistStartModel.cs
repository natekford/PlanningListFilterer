using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistStartModel(
	int? Year,
	int? Month
)
{
	[JsonIgnore]
	public DateTime? Time
	{
		get
		{
			if (Year is not int year)
			{
				return null;
			}
			return new(year: year, month: Month ?? 12, day: 1);
		}
	}
}