namespace PlanningListFilterer.Models.Anilist.Filter;

public sealed class AnilistFilterGenres : AnilistFilterValues<string>
{
	public AnilistFilterGenres(AnilistFilterer parent) : base(parent)
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