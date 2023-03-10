namespace PlanningListFilterer.Models.Anilist.Filter;

public abstract class AnilistFilter
{
	protected AnilistFilterer Parent { get; }

	protected AnilistFilter(AnilistFilterer parent)
	{
		Parent = parent;
	}

	public abstract bool IsValid(AnilistModel model);

	public abstract void Reset();
}