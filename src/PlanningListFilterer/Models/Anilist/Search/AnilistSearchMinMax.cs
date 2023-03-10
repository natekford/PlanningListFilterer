namespace PlanningListFilterer.Models.Anilist.Search;

public sealed class AnilistSearchMinMax : AnilistSearchItem
{
	private readonly Func<AnilistModel, int?> _GetProperty;

	public int? Max { get; private set; }
	public int? Min { get; private set; }

	public AnilistSearchMinMax(
		AnilistSearch search,
		Func<AnilistModel, int?> getProperty) : base(search)
	{
		_GetProperty = getProperty;
	}

	public override void Reset()
	{
		Max = null;
		Min = null;
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

	public Task SetMax(int? max)
	{
		Max = max;
		return Search.UpdateVisibilityAsync();
	}

	public Task SetMin(int? min)
	{
		Min = min;
		return Search.UpdateVisibilityAsync();
	}
}