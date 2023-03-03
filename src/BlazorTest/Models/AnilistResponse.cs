using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistResponse(
	[property: JsonPropertyName("data")]
	AnilistData Data
)
{
	internal static JsonSerializerOptions JsonOptions { get; } = CreateJsonOptions();

	public const string GRAPHQL_URL = "https://graphql.anilist.co";
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

	public static async Task<AnilistResponse> GetAnilistResponseAsync(
		HttpClient client,
		string username)
	{
		var body = JsonSerializer.Serialize(new
		{
			query = GRAPHQL_QUERY,
			variables = new
			{
				username = username,
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

		using var response = await client.SendAsync(request).ConfigureAwait(false);
		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

		return (await JsonSerializer.DeserializeAsync<AnilistResponse>(
			utf8Json: stream,
			options: JsonOptions
		).ConfigureAwait(false))!;
	}

	private static JsonSerializerOptions CreateJsonOptions()
	{
		var options = new JsonSerializerOptions();
		options.Converters.Add(new JsonStringEnumConverter());
		return options;
	}
}