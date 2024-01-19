namespace PlanningListFilterer.Models.Anilist;

public readonly struct AnilistUsername
{
	public bool IsValid { get; }
	public string ListKey { get; }
	public string MetaKey { get; }
	public string Name { get; }

	public AnilistUsername(string username)
	{
		Name = username?.ToLower() ?? "";
		ListKey = $"LIST_{Name}";
		MetaKey = $"META_{Name}";
		// TODO: implement whatever regex anilist uses
		IsValid = !string.IsNullOrWhiteSpace(username);
	}
}