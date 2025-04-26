using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Settings;

public sealed class ListSettings
{
	public bool AutomaticallyToggleFriendScoreColumns { get; set; } = true;
	public bool AutomaticallyToggleGlobalScoreColumns { get; set; } = true;
	public bool EnableFriendScores { get; set; }
	public AnilistMediaListStatus ListStatus { get; set; } = AnilistMediaListStatus.PLANNING;
}