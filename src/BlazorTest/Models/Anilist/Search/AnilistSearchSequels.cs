namespace BlazorTest.Models.Anilist.Search;

public sealed class AnilistSearchSequels : AnilistSearchBool
{
	public AnilistSearchSequels(AnilistSearch search) : base(search)
	{
	}

	public override bool IsValid(AnilistModel model)
		=> !Value || !model.IsSequel;

	public override void Reset()
		=> Value = false;
}