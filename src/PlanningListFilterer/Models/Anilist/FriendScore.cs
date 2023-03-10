using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public record struct FriendScore(
	AnilistMedia Media,
	int? Score
);