using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public record struct AnilistFriendScore(
	AnilistMedia Media,
	int? Score,
	int Popularity
);