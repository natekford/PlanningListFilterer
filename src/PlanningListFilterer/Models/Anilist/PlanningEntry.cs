using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public record struct PlanningEntry(
	AnilistUser User,
	AnilistMedia Media
);
