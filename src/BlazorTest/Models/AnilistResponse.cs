using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorTest.Models;

public sealed record AnilistResponse(
	[property: JsonPropertyName("data")]
	Data Data
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
					...mediaListEntry
				}
			}
		}
	}

	fragment mediaListEntry on MediaList {
		media {
			id
			title {
				userPreferred
			}
			format
			episodes
			nextAiringEpisode {
				episode
			}
			duration
			averageScore
			popularity
			startDate {
				year
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
};

public sealed record Data(
	[property: JsonPropertyName("MediaListCollection")]
	MediaListCollection MediaListCollection
);

public sealed record MediaListCollection(
	[property: JsonPropertyName("lists")]
	EntryList[] Lists
);

public sealed record EntryList(
	[property: JsonPropertyName("name")]
	string Name,
	[property: JsonPropertyName("isCustomList")]
	bool IsCustomList,
	[property: JsonPropertyName("isCompletedList")]
	bool IsCompletedList,
	[property: JsonPropertyName("entries")]
	Entry[] Entries
);

public sealed record Entry(
	[property: JsonPropertyName("media")]
	Media Media
);