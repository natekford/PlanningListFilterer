using System.Net.Mime;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using BlazorTest.Models.Anilist.Json;

namespace BlazorTest.Models.Anilist;

public static class AnilistUtils
{
	public const string GRAPHQL_QUERY = @"
	query ($username: String) {
		MediaListCollection(userName: $username, type: ANIME, status: PLANNING) {
			lists {
				name
				isCustomList
				isCompletedList: isSplitCompletedList
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
						genres
						tags {
							name
							rank
						}
					}
				}
			}
		}
	}
	";
	public const string GRAPHQL_URL = "https://graphql.anilist.co";
	public const string NO_VALUE = "N/A";

	public static string DisplayDuration(this AnilistViewModel media)
	{
		var duration = media.GetTotalDuration();
		if (!duration.HasValue)
		{
			return NO_VALUE;
		}
		return $"{duration} minute{(duration == 1 ? "" : "s")}";
	}

	public static string DisplayEpisodeCount(this AnilistViewModel media)
	{
		var count = media.GetHighestEpisode();
		if (!count.HasValue)
		{
			return NO_VALUE;
		}
		return $"{count} episode{(count == 1 ? "" : "s")}";
	}

	public static string DisplayFormat(this AnilistViewModel media)
		=> media.Format?.ToString() ?? NO_VALUE;

	public static string DisplayGenres(this AnilistViewModel media, bool expanded)
		=> media.Genres.DisplayExpandable(expanded);

	public static string DisplayScore(this AnilistViewModel media)
	{
		var score = media.AverageScore;
		if (!score.HasValue)
		{
			return NO_VALUE;
		}
		return $"{score}%";
	}

	public static string DisplayStart(this AnilistViewModel media)
	{
		var year = media.StartYear;
		if (!year.HasValue)
		{
			return NO_VALUE;
		}

		var month = media.StartMonth;
		var format = month.HasValue ? "yyyy MMM" : "yyyy";
		return media.Start!.Value.ToString(format);
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

	public static string DisplayTags(this AnilistViewModel media, bool expanded)
	{
		var tags = media.Tags
			.OrderByDescending(x => x.Value)
			.Select(x => x.DisplayTag());
		return tags.DisplayExpandable(expanded);
	}

	public static async Task<AnilistResponse> GetAnilistAsync(
		this HttpClient http,
		string username)
	{
		var body = JsonSerializer.Serialize(new
		{
			query = GRAPHQL_QUERY,
			variables = new
			{
				username,
			}
		});
		var content = new StringContent(
			content: body,
			encoding: Encoding.UTF8,
			mediaType: MediaTypeNames.Application.Json
		);

		var request = new HttpRequestMessage(
			method: HttpMethod.Post,
			requestUri: GRAPHQL_URL
		)
		{
			Content = content
		};
		request.Headers.Add("Accept", MediaTypeNames.Application.Json);

		using var response = await http.SendAsync(request).ConfigureAwait(false);
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

	public static int? GetHighestEpisode(this AnilistViewModel media)
		=> media.Episodes ?? media.NextAiringEpisode;

	public static int? GetTotalDuration(this AnilistViewModel media)
		=> media.GetHighestEpisode() * media.Duration;

	public static string GetUrl(this AnilistViewModel media)
		=> $"https://anilist.co/anime/{media.Id}/";

	private static string DisplayExpandable(this IEnumerable<string> items, bool expanded)
	{
		if (!items.Any())
		{
			return NO_VALUE;
		}

		if (expanded)
		{
			return string.Join(Environment.NewLine, items.Skip(1));
		}
		return items.First();
	}
}