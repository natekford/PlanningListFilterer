using PlanningListFilterer.Models.Anilist.Json;

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistModel(
	int Id,
	string Title,
	AnilistMediaStatus Status,
	AnilistMediaFormat Format,
	int? Episodes,
	int? Duration,
	int? Score,
	int? FriendScore,
	int Popularity,
	int? Year,
	int? Month,
	string? CoverImageUrl,
	ImmutableHashSet<string> Genres,
	ImmutableDictionary<string, int> Tags,
	bool IsSequel
) : IFuzzyDate
{
	[JsonIgnore]
	public DateTime Start => this.GetDate();
	[JsonIgnore]
	public IReadOnlyList<string> TagNames => Tags.Keys.ToList();

	public static AnilistModel Create(AnilistMedia media, int? friendScore)
	{
		var year = media.StartDate?.Year;
		var month = media.StartDate?.Month;
		var start = AnilistModelUtils.GetDate(year, month);
		var episodes = media.Episodes ?? media.NextAiringEpisode?.Episode;
		return new(
			Id: media.Id,
			Title: media.Title.UserPreferred,
			Status: media.Status ?? AnilistMediaStatus.UNKNOWN,
			Format: media.Format ?? AnilistMediaFormat.UNKNOWN,
			Episodes: episodes,
			Duration: episodes * media.Duration,
			Score: media.AverageScore,
			FriendScore: friendScore,
			Popularity: media.Popularity,
			Year: year,
			Month: month,
			CoverImageUrl: media.CoverImage?.Medium,
			Genres: media.Genres.ToImmutableHashSet(),
			Tags: media.Tags.ToImmutableDictionary(
				keySelector: x => x.Name,
				elementSelector: x => x.Rank
			),
			IsSequel: media.Relations.Edges.Any(x =>
			{
				return (x.RelationType == AnilistMediaRelation.PREQUEL
					|| x.RelationType == AnilistMediaRelation.PARENT)
					&& x.Node.Type == AnilistMediaType.ANIME
					&& (x.Node.StartDate?.GetDate() < start);
			})
		);
	}
}