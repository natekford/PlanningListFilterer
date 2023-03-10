namespace BlazorTest.Models.Anilist.Search;

public abstract class AnilistSearchBool : AnilistSearchItem
{
	public bool Value { get; protected set; }

	protected AnilistSearchBool(AnilistSearch search) : base(search)
	{
	}

	public Task SetValue(bool value)
	{
		Value = value;
		return Search.UpdateVisibilityAsync();
	}
}