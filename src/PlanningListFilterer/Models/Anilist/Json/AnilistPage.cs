using System.Text.Json.Serialization;

namespace PlanningListFilterer.Models.Anilist.Json;

public sealed record AnilistPage(
	[property: JsonPropertyName("pageInfo")]
	AnilistPageInfo PageInfo,
	[property: JsonPropertyName("mediaList")]
	AnilistMediaList[] MediaList,
	[property: JsonPropertyName("following")]
	AnilistUser[] Following
);