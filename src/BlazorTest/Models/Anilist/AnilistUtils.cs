using System.Text.Json;
using System.Net.Http.Json;
using BlazorTest.Models.Anilist.Json;

namespace BlazorTest.Models.Anilist;

public static class AnilistUtils
{
	public const string GRAPHQL_LIST_QUERY = @"
	query ($username: String, $chunk: Int) {
		MediaListCollection(userName: $username, type: ANIME, status: PLANNING, perChunk: 500, chunk: $chunk) {
			hasNextChunk
			lists {
				entries {
					media {
						id
						title {
							userPreferred
						}
						status
						format
						episodes
						duration
						averageScore
						popularity
						startDate {
							year,
							month
						}
						coverImage {
							medium
						}
						genres
						tags {
							name
							rank
						}
						relations {
							edges {
								node {
									id
									type
									startDate {
										year
										month
									}
								}
								relationType
							}
						}
					}
				}
			}
		}
	}
	";
	public const string GRAPHQL_URL = "https://graphql.anilist.co";
	public const string NO_VALUE = "N/A";

	public static AnilistStartModel CreateStartModel(this AnilistMedia media)
	{
		return new(
			Year: media.StartDate?.Year,
			Month: media.StartDate?.Month
		);
	}

	public static string DisplayDuration(this AnilistModel model)
	{
		var duration = model.GetTotalDuration();
		return duration.HasValue ? duration.Value.ToString() : NO_VALUE;
	}

	public static string DisplayEpisodeCount(this AnilistModel model)
	{
		var count = model.GetHighestEpisode();
		return count.HasValue ? count.Value.ToString() : NO_VALUE;
	}

	public static string DisplayFormat(this AnilistModel model)
		=> model.Format?.ToString() ?? NO_VALUE;

	public static string DisplayGenres(this AnilistModel model)
		=> model.Genres.DisplayStrings();

	public static string DisplayScore(this AnilistModel model)
	{
		var score = model.AverageScore;
		return score.HasValue ? $"{score}%" : NO_VALUE;
	}

	public static string DisplayStart(this AnilistModel model)
	{
		var start = model.Start;
		if (!start.Year.HasValue)
		{
			return NO_VALUE;
		}

		var format = start.Month.HasValue ? "yyyy MMM" : "yyyy";
		return start.Time!.Value.ToString(format);
	}

	public static string DisplayTag(this KeyValuePair<string, int> tag)
	{
		var name = tag.Key;
		if (name is "Cute Girls Doing Cute Things")
		{
			name = "CGDCT";
		}
		return $"{name} ({tag.Value}%)";
	}

	public static string DisplayTags(this IEnumerable<KeyValuePair<string, int>> tags)
		=> tags.Select(x => x.DisplayTag()).DisplayStrings();

	public static string DisplayTags(this AnilistModel model, int skip, int count)
	{
		return model.Tags
			.OrderByDescending(x => x.Value)
			.Skip(skip)
			.Take(count)
			.DisplayTags();
	}

	public static async IAsyncEnumerable<AnilistMedia> GetAnilistAsync(
		this HttpClient http,
		string username)
	{
		var chunk = 1;
		AnilistMediaListCollection collection;
		do
		{
			var response = await http.GetAnilistResponseAsync(username, chunk).ConfigureAwait(false);
			collection = response.Data.MediaListCollection;

			foreach (var list in collection.Lists)
			{
				foreach (var entry in list.Entries)
				{
					yield return entry.Media;
				}
			}

			++chunk;
		} while (collection.HasNextChunk);
	}

	public static async Task<AnilistResponse> GetAnilistResponseAsync(
		this HttpClient http,
		string username,
		int chunk)
	{
		var body = new
		{
			query = GRAPHQL_LIST_QUERY,
			variables = new
			{
				username = username,
				chunk = chunk
			}
		};
		using var response = await http.PostAsJsonAsync(GRAPHQL_URL, body).ConfigureAwait(false);
		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

		return (await JsonSerializer.DeserializeAsync<AnilistResponse>(
			utf8Json: stream
		).ConfigureAwait(false))!;
	}

	public static async Task<AnilistResponse> GetAnilistSampleAsync(
		this HttpClient http)
	{
		return (await http.GetFromJsonAsync<AnilistResponse>(
			requestUri: $"sample-data/anilistresponse.json?a={Guid.NewGuid()}"
		).ConfigureAwait(false))!;
	}

	public static int? GetHighestEpisode(this AnilistModel model)
		=> model.Episodes ?? model.NextAiringEpisode;

	public static int? GetTotalDuration(this AnilistModel model)
		=> model.GetHighestEpisode() * model.Duration;

	public static string GetUrl(this AnilistModel model)
		=> $"https://anilist.co/anime/{model.Id}/";

	private static string DisplayStrings(this IEnumerable<string> items)
	{
		if (!items.Any())
		{
			return NO_VALUE;
		}
		return string.Join(Environment.NewLine, items);
	}
}