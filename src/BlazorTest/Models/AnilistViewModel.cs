using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistViewModelList(
	List<AnilistViewModel> Entries,
	DateTime SavedAt
);

public sealed record AnilistViewModel(
	int Id,
	string Title,
	AnilistMediaStatus? Status,
	AnilistMediaFormat? Format,
	int? Episodes,
	int? NextAiringEpisode,
	int? Duration,
	int? AverageScore,
	int Popularity,
	int? StartYear,
	int? StartMonth,
	ImmutableHashSet<string> Genres,
	IReadOnlyDictionary<string, int> Tags
)
{
	[JsonIgnore]
	public bool IsEntryVisible { get; set; } = true;
	[JsonIgnore]
	public DateTime? Start
	{
		get
		{
			if (StartYear is not int year)
			{
				return null;
			}
			return new DateTime(year: year, month: StartMonth ?? 12, day: 1);
		}
	}

	public static AnilistViewModel FromMedia(AnilistMedia media)
	{
		return new(
			Id: media.Id,
			Title: media.Title.UserPreferred,
			Status: media.Status,
			Format: media.Format,
			Episodes: media.Episodes,
			NextAiringEpisode: media.NextAiringEpisode?.Episode,
			Duration: media.Duration,
			AverageScore: media.AverageScore,
			Popularity: media.Popularity,
			StartYear: media.StartDate?.Year,
			StartMonth: media.StartDate?.Month,
			Genres: media.Genres.ToImmutableHashSet(),
			Tags: media.Tags.ToImmutableDictionary(x => x.Name, x => x.Rank)
		);
	}
}