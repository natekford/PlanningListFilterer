namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistMeta(
	DateTime SavedAt,
	int Version
)
{
	public const int CURRENT_VERSION = 6;

	public bool IsOutOfDate(TimeSpan limit)
		=> Version < CURRENT_VERSION || (DateTime.UtcNow - SavedAt) > limit;

	public static AnilistMeta New()
	{
		return new(
			SavedAt: DateTime.UtcNow,
			Version: CURRENT_VERSION
		);
	}
}