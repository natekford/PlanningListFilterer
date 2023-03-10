namespace PlanningListFilterer.Models.Anilist.Filter;

public sealed class AnilistFilterSequels : AnilistFilterBool
{
	public AnilistFilterSequels(AnilistFilterer search) : base(search)
	{
	}

	public override bool IsValid(AnilistModel model)
		=> !Value || !model.IsSequel;

	public override void Reset()
		=> Value = false;
}