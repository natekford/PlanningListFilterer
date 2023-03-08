using BlazorTest.Models.Anilist.Json;

namespace BlazorTest.Models.Anilist;

public sealed record AnilistRelationModel(
	int Id,
	AnilistMediaRelation Relation,
	AnilistStartModel Start
);