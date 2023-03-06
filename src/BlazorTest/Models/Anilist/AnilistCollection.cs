namespace BlazorTest.Models.Anilist;

public sealed record AnilistCollection(
	List<AnilistModel> Entries,
	DateTime SavedAt
);