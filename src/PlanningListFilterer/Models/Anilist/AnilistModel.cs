using PlanningListFilterer.Models.Anilist.Json;

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
	int Popularity,
	int? FriendScore,
	int FriendPopularityScored,
	int FriendPopularityTotal,
	int? Year,
	int? Month,
	string? CoverImageUrl,
	// Immutable* is slow. I would use Frozen*, but
	// * System.Text.Json doesn't deserialize them
	// * I don't want to write a converter for them
	// * The speed is similar to regular collections
	HashSet<string> Genres,
	Dictionary<string, int> Tags,
	bool IsSequel
) : IFuzzyDate
{
	[JsonIgnore]
	public DateTime Start => this.GetDate();

	public static AnilistModel Create(AnilistMedia media)
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
			Popularity: media.Popularity,
			FriendScore: default,
			FriendPopularityScored: default,
			FriendPopularityTotal: default,
			Year: year,
			Month: month,
			CoverImageUrl: media.CoverImage?.Medium,
			Genres: [.. media.Genres],
			Tags: media.Tags.ToDictionary(
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