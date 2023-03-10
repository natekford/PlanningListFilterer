namespace PlanningListFilterer.Models.Anilist.Filter;

public abstract class AnilistFilterBool : AnilistFilter
{
	public bool Value { get; protected set; }

	protected AnilistFilterBool(AnilistFilterer parent) : base(parent)
	{
	}

	public Task SetValue(bool value)
	{
		Value = value;
		return Parent.UpdateVisibilityAsync();
	}
}