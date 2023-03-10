namespace PlanningListFilterer.Models.Anilist.Filter;

public sealed class AnilistFilterTags : AnilistFilterValues<string>
{
	public AnilistFilterTags(AnilistFilterer search) : base(search)
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