namespace BlazorTest.Models.Anilist.Search;

public interface IAnilistSearchItem
{
	void Clear();

	bool IsValid(AnilistModel model);
}