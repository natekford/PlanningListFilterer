namespace BlazorTest.Models.Anilist;

public sealed record AnilistCollection(
	DateTime SavedAt,
	int Version,
	List<AnilistModel> Entries
)
{
	public const int CURRENT_VERSION = 1;

	public bool IsOutOfDate(TimeSpan limit)
		=> Version < CURRENT_VERSION || (DateTime.UtcNow - SavedAt) > limit;

	public static AnilistCollection Create(List<AnilistModel> entries)
		=> new(DateTime.UtcNow, CURRENT_VERSION, entries);
}