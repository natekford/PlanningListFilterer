using System.Text.RegularExpressions;

namespace PlanningListFilterer.Models.Anilist;

public readonly partial struct AnilistUsername
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
		IsValid = AnilistUsernameRegex().IsMatch(Name);
	}

	// Length has to be at least 2 characters but not more than 20
	// Letters/numbers allowed, no underscore or other symbols
	[GeneratedRegex("^[a-zA-Z0-9]{2,20}$")]
	private static partial Regex AnilistUsernameRegex();
}