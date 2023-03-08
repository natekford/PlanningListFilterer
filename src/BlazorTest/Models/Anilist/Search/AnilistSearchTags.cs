namespace BlazorTest.Models.Anilist.Search;

public sealed class AnilistSearchTags : AnilistSearchValues<string>
{
	public AnilistSearchTags(AnilistSearch search) : base(search)
	{
	}

	public override bool IsValid(AnilistModel model)
	{
		foreach (var value in Values)
		{
			if (!model.Tags.ContainsKey(value))
			{
				return false;
			}
		}
		return true;
	}
}