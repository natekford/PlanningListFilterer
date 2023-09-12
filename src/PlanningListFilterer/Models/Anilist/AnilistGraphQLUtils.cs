using PlanningListFilterer.Models.Anilist.Json;

using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;

namespace PlanningListFilterer.Models.Anilist;

public static class AnilistGraphQLUtils
{
	public const int CHUNK_SIZE = 500;
	public const string GRAPHQL_URL = "https://graphql.anilist.co";
	public const int PAGE_SIZE = 50;

	public static async IAsyncEnumerable<AnilistUser> GetAnilistFollowingAsync(
		this HttpClient http,
		AnilistUser user)
	{
		AnilistPage page;

		var pageNumber = 1;
		do
		{
			page = await http.GetAnilistFollowersPageAsync(
				userId: user.Id,
				page: pageNumber
			).ConfigureAwait(false);

			foreach (var following in page.Following)
			{
				yield return following;
			}

			++pageNumber;
		} while (page.PageInfo.HasNextPage);
	}

	public static async IAsyncEnumerable<AnilistFriendScore> GetAnilistFriendScoresAsync(
		this HttpClient http,
		IEnumerable<AnilistMedia> media,
		IEnumerable<AnilistUser> users)
	{
		var mediaDict = media.ToDictionary(x => x.Id, x => x);
		// concurrentdict solely because getoradd exists
		var pagesOfMediaIds = new ConcurrentDictionary<int, HashSet<int>>
		{
			[1] = media.Select(x => x.Id).ToHashSet(),
		};
		var userIds = users.Select(x => x.Id).ToList();
		var storage = new Dictionary<int, FriendScoreTally>();

		for (var pageNumber = 1; pagesOfMediaIds.GetValueOrDefault(pageNumber)?.Count > 0; ++pageNumber)
		{
			var mediaIds = pagesOfMediaIds[pageNumber];
			do
			{
				// only do around 150 at a time because the query length has a max
				// complexity its allowed (500, each media id adds 3)
				var pages = await http.GetAnilistFriendScoresPageAsync(
					mediaIds: mediaIds.Take(150),
					userIds: userIds,
					page: pageNumber
				).ConfigureAwait(false);
				foreach (var (key, page) in pages)
				{
					// we name the property/key in the graphql query as _{id}
					var id = int.Parse(key[1..]);
					var tally = storage.GetValueOrDefault(id);
					foreach (var entry in page.MediaList)
					{
						if (entry.Score == 0)
						{
							++tally.UnscoredCount;
						}
						else
						{
							++tally.ScoredCount;
							tally.ScoreSum += entry.Score;
						}
					}

					// remove no matter what because if we need to go to a later
					// page the first condition adds the id to the next page's set,
					// and if we don't need to go to a later page we are done with
					// processing the id
					mediaIds.Remove(id);
					// graphql query only returns the users who have the anime on
					// their list, e.g.
					// 1000 followers, 2 people with anime, only 2 in list
					// 51 followers, 51 people with anime, 50 in list, must go to next
					// page to get the last one
					// 2nd condition is to deal with something like
					// 50 followers, 50 people with anime, 50 in list, no need to go
					// to next page since 1 page is all we needed
					if (page.MediaList.Length == PAGE_SIZE
						&& (pageNumber * PAGE_SIZE) < userIds.Count)
					{
						storage[id] = tally;
						pagesOfMediaIds.GetOrAdd(pageNumber + 1, _ => new()).Add(id);
					}
					else
					{
						storage.Remove(id);
						yield return new(mediaDict[id], tally.AverageScore, tally.TotalCount);
					}
				}
			} while (mediaIds.Count > 0);
		}
	}

	public static async IAsyncEnumerable<AnilistPlanningEntry> GetAnilistPlanningListAsync(
		this HttpClient http,
		string username)
	{
		AnilistMediaListCollection collection;

		var chunk = 1;
		do
		{
			collection = await http.GetAnilistPlanningListChunkAsync(
				username: username,
				chunk: chunk
			).ConfigureAwait(false);

			foreach (var list in collection.Lists)
			{
				foreach (var entry in list.Entries)
				{
					yield return new(collection.User, entry.Media);
				}
			}

			++chunk;
		} while (collection.HasNextChunk);
	}

	internal static async Task<AnilistMediaListCollection> GetAnilistSampleAsync(
		this HttpClient http)
	{
		return (await http.GetFromJsonAsync<AnilistResponse<QueryPlanningList>>(
			requestUri: $"sample-data/anilistresponse.json?a={Guid.NewGuid()}"
		).ConfigureAwait(false))!.Data.MediaListCollection;
	}

	private static async Task<AnilistPage> GetAnilistFollowersPageAsync(
		this HttpClient http,
		int userId,
		int page
	)
	{
		var query = $@"
		query ($userId: Int!, $page: Int) {{
			Page(page: $page, perPage: {PAGE_SIZE}) {{
				pageInfo {{
					hasNextPage
				}}
				following(userId: $userId) {{
					id
				}}
			}}
		}}
		";

		return (await http.GetResponseAsync<Dictionary<string, AnilistPage>>(new
		{
			query = query,
			variables = new
			{
				userId = userId,
				page = page,
			},
		}).ConfigureAwait(false)).Single().Value;
	}

	private static async Task<Dictionary<string, AnilistPage>> GetAnilistFriendScoresPageAsync(
		this HttpClient http,
		IEnumerable<int> mediaIds,
		IEnumerable<int> userIds,
		int page)
	{
		var query = @"
		query FollowingScore($userIds: [Int], $page: Int) {" + string.Join("\n", mediaIds.Select(id =>
		{
			return $@"
			_{id}: Page(page: $page, perPage: {PAGE_SIZE}) {{
				mediaList(mediaId: {id}, status_not: PLANNING, userId_in: $userIds) {{
					...sc
				}}
			}}
			";
		})) +
		@"
		}

		fragment sc on MediaList {
			score(format: POINT_100)
		}
		";

		return await http.GetResponseAsync<Dictionary<string, AnilistPage>>(new
		{
			query = query,
			variables = new
			{
				userIds = userIds,
				page = page,
			},
		}).ConfigureAwait(false);
	}

	private static async Task<AnilistMediaListCollection> GetAnilistPlanningListChunkAsync(
		this HttpClient http,
		string username,
		int chunk)
	{
		var query = $@"
		query ($username: String, $chunk: Int) {{
			MediaListCollection(userName: $username, type: ANIME, status: PLANNING, perChunk: {CHUNK_SIZE}, chunk: $chunk) {{
				hasNextChunk
				user {{
					id
				}}
				lists {{
					entries {{
						media {{
							id
							title {{
								userPreferred
							}}
							status
							format
							episodes
							duration
							averageScore
							popularity
							startDate {{
								year,
								month
							}}
							coverImage {{
								medium
							}}
							genres
							tags {{
								name
								rank
							}}
							relations {{
								edges {{
									node {{
										id
										type
										startDate {{
											year
											month
										}}
									}}
									relationType
								}}
							}}
						}}
					}}
				}}
			}}
		}}
		";

		return (await http.GetResponseAsync<QueryPlanningList>(new
		{
			query = query,
			variables = new
			{
				username = username,
				chunk = chunk,
			},
		}).ConfigureAwait(false)).MediaListCollection;
	}

	private static async Task<T> GetResponseAsync<T>(
		this HttpClient http,
		object body)
	{
		using var response = await http.PostAsJsonAsync(GRAPHQL_URL, body).ConfigureAwait(false);
		using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

		return (await JsonSerializer.DeserializeAsync<AnilistResponse<T>>(
			utf8Json: stream
		).ConfigureAwait(false))!.Data;
	}

	private record struct FriendScoreTally(
		int ScoredCount,
		int UnscoredCount,
		double ScoreSum
	)
	{
		public readonly int TotalCount => ScoredCount + UnscoredCount;
		public readonly int? AverageScore
		{
			get
			{
				var avg = (int)(ScoreSum / Math.Max(1, ScoredCount));
				// avg score of 0 means no score
				return avg == 0 ? null : avg;
			}
		}
	}
}