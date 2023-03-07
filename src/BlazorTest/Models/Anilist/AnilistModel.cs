using BlazorTest.Models.Anilist.Json;

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace BlazorTest.Models.Anilist;

public sealed record AnilistModel(
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
	string? CoverImageUrl,
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

	public static AnilistModel Create(AnilistMedia media)
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
			CoverImageUrl: media.CoverImage?.Medium,
			Genres: media.Genres.ToImmutableHashSet(),
			Tags: media.Tags.ToImmutableDictionary(x => x.Name, x => x.Rank)
		);
	}
}