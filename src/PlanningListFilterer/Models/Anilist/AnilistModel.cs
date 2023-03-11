using PlanningListFilterer.Models.Anilist.Json;

using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistModel(
	int Id,
	string Title,
	AnilistMediaStatus? Status,
	AnilistMediaFormat? Format,
	int? Episodes,
	int? Duration,
	int? Score,
	int? FriendScore,
	int Popularity,
	AnilistStartModel Start,
	string? CoverImageUrl,
	ImmutableHashSet<string> Genres,
	ImmutableDictionary<string, int> Tags,
	bool IsSequel
)
{
	[JsonIgnore]
	public bool IsVisible { get; set; } = true;
	[JsonIgnore]
	public int? TotalDuration => Episodes * Duration;

	public static AnilistModel Create(AnilistMedia media, int? friendScore)
	{
		var start = media.CreateStartModel();
		return new(
			Id: media.Id,
			Title: media.Title.UserPreferred,
			Status: media.Status,
			Format: media.Format,
			Episodes: media.Episodes ?? media.NextAiringEpisode?.Episode,
			Duration: media.Duration,
			Score: media.AverageScore,
			FriendScore: friendScore,
			Popularity: media.Popularity,
			Start: start,
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
					&& x.Node.CreateStartModel().Time < start.Time;
			})
		);
	}
}