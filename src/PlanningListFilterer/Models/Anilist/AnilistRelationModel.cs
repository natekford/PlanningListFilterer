using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistRelationModel(
	int Id,
	AnilistMediaRelation Relation,
	AnilistStartModel Start
);