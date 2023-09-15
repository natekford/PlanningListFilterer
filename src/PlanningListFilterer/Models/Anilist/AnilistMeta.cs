using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistMeta(
	DateTime SavedAt,
	int Version,
	int UserId,
	bool SavedWithFriendScores
)
{
	public const int CURRENT_VERSION = 11;

	public bool ShouldReacquire(ListSettings settings, TimeSpan limit)
	{
		return Version < CURRENT_VERSION
			|| (DateTime.UtcNow - SavedAt) > limit
			|| (settings.EnableFriendScores && !SavedWithFriendScores);
	}

	public AnilistMeta() : this(default, default, default, default)
	{
	}

	public AnilistMeta(int userId, ListSettings settings) : this(
		SavedAt: DateTime.UtcNow,
		Version: CURRENT_VERSION,
		UserId: userId,
		SavedWithFriendScores: settings.EnableFriendScores
	)
	{
	}
}