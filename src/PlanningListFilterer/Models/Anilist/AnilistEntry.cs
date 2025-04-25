using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public record struct AnilistEntry(
	AnilistUser User,
	AnilistMedia Media
);