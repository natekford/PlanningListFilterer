using BlazorTest.Models.Anilist.Json;

namespace BlazorTest.Models.Anilist.Search;

public sealed class AnilistSearchFormats : AnilistSearchValues<AnilistMediaFormat?>
{
	public AnilistSearchFormats(AnilistSearch search) : base(search)
	{
	}

	public override bool IsValid(AnilistModel model)
		=> model.Format is not AnilistMediaFormat format
			|| Values.Count == 0 || Values.Contains(format);
}