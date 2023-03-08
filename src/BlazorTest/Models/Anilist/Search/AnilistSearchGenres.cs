namespace BlazorTest.Models.Anilist.Search;

public sealed class AnilistSearchGenres : AnilistSearchValues<string>
{
	public AnilistSearchGenres(AnilistSearch search) : base(search)
	{
	}

	public override bool IsValid(AnilistModel model)
	{
		foreach (var value in Values)
		{
			if (!model.Genres.Contains(value))
			{
				return false;
			}
		}
		return true;
	}
}