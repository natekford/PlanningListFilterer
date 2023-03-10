namespace PlanningListFilterer.Models.Anilist.Filter;

public sealed class AnilistFilterMinMax : AnilistFilter
{
	private readonly Func<AnilistModel, int?> _GetProperty;

	public int? Max { get; private set; }
	public int? Min { get; private set; }

	public AnilistFilterMinMax(
		AnilistFilterer parent,
		Func<AnilistModel, int?> getProperty) : base(parent)
	{
		_GetProperty = getProperty;
	}

	public override bool IsValid(AnilistModel model)
	{
		// no value means to not bother checking it
		if (_GetProperty(model) is not int value)
		{
			return true;
		}
		return (Min is null || Min <= value) && (Max is null || Max >= value);
	}

	public override void Reset()
	{
		Max = null;
		Min = null;
	}

	public Task SetMax(int? max)
	{
		Max = max;
		return Parent.UpdateVisibilityAsync();
	}

	public Task SetMin(int? min)
	{
		Min = min;
		return Parent.UpdateVisibilityAsync();
	}
}