using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist.Filter;

// has to be nullable otherwise multiselect only shows the 1st enum
public sealed class AnilistFilterFormats : AnilistFilterValues<AnilistMediaFormat?>
{
	public AnilistFilterFormats(AnilistFilterer parent) : base(parent)
	{
	}

	public override bool IsValid(AnilistModel model)
		=> model.Format is not AnilistMediaFormat format
			|| Values.Count == 0 || Values.Contains(format);
}