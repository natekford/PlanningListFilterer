using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Models.Anilist;

public sealed record AnilistMeta(
	DateTime SavedAt,
	int Version,
	int Length,
	int UserId,
	bool SavedWithFriendScores
)
{
	public const int CURRENT_VERSION = 12;

	public bool ShouldReacquire(ListSettings settings, TimeSpan limit)
	{
		return Version < CURRENT_VERSION
			|| (DateTime.UtcNow - SavedAt) > limit
			|| (settings.EnableFriendScores && !SavedWithFriendScores);
	}

	public AnilistMeta() : this(default, default, default, default, default)
	{
	}

	public AnilistMeta(int userId, int length, ListSettings settings) : this(
		SavedAt: DateTime.UtcNow,
		Version: CURRENT_VERSION,
		Length: length,
		UserId: userId,
		SavedWithFriendScores: settings.EnableFriendScores
	)
	{
	}
}

public sealed class AnilistMetaCollection : SortedDictionary<string, AnilistMeta>;