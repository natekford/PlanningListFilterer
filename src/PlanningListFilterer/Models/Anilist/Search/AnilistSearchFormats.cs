using PlanningListFilterer.Models.Anilist.Json;

namespace PlanningListFilterer.Models.Anilist.Search;

// has to be nullable otherwise multiselect only shows the 1st enum
public sealed class AnilistSearchFormats : AnilistSearchValues<AnilistMediaFormat?>
{
	public AnilistSearchFormats(AnilistSearch search) : base(search)
	{
	}

	public override bool IsValid(AnilistModel model)
		=> model.Format is not AnilistMediaFormat format
			|| Values.Count == 0 || Values.Contains(format);
}