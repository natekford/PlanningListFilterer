namespace BlazorTest.Models.Anilist.Search;

public class AnilistSearchMinMax : IAnilistSearchItem
{
	private readonly Func<AnilistModel, int?> _GetProperty;
	private readonly AnilistSearch _Search;

	public int? Max { get; private set; }
	public int? Min { get; private set; }

	public AnilistSearchMinMax(
		AnilistSearch search,
		Func<AnilistModel, int?> getProperty)
	{
		_Search = search;
		_GetProperty = getProperty;
	}

	public virtual bool IsValid(AnilistModel model)
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
		return _Search.UpdateVisibilityAsync();
	}

	public Task SetMin(int? min)
	{
		Min = min;
		return _Search.UpdateVisibilityAsync();
	}

	void IAnilistSearchItem.Clear()
	{
		Max = null;
		Min = null;
	}
}