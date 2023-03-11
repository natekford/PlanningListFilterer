using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistMeta(
	DateTime SavedAt,
	int Version,
	int UserId,
	bool SavedWithFriendScores
)
{
	public const int CURRENT_VERSION = 10;

	public bool ShouldReacquire(ListSettings settings, TimeSpan limit)
	{
		return Version < CURRENT_VERSION
			|| (DateTime.UtcNow - SavedAt) > limit
			|| (settings.EnableFriendScores && !SavedWithFriendScores);
	}

	public static AnilistMeta New(int userId, bool savedWithFriendScores)
	{
		return new(
			SavedAt: DateTime.UtcNow,
			Version: CURRENT_VERSION,
			UserId: userId,
			SavedWithFriendScores: savedWithFriendScores
		);
	}
}