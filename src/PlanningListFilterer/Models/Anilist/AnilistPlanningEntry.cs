using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public record struct AnilistPlanningEntry(
	AnilistUser User,
	AnilistMedia Media
);