namespace BlazorTest.Models.Anilist;

public sealed record AnilistMeta(
	DateTime SavedAt,
	int Version,
	int Length
)
{
	public const int CURRENT_VERSION = 5;

	public bool IsOutOfDate(TimeSpan limit)
		=> Version < CURRENT_VERSION || (DateTime.UtcNow - SavedAt) > limit;

	public static AnilistMeta Create(IReadOnlyList<AnilistModel> list)
	{
		return new(
			SavedAt: DateTime.UtcNow,
			Version: CURRENT_VERSION,
			Length: list.Count
		);
	}
}