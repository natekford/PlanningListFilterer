namespace BlazorTest.Models.Anilist;

public sealed record AnilistViewModelList(
	List<AnilistViewModel> Entries,
	DateTime SavedAt
);