namespace BlazorTest.Models.Anilist.Search;

public abstract class AnilistSearchItem
{
	protected AnilistSearch Search { get; }

	protected AnilistSearchItem(AnilistSearch search)
	{
		Search = search;
	}

	public abstract void Reset();

	public abstract bool IsValid(AnilistModel model);
}