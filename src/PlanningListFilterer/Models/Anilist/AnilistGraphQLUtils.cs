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
		AnilistUser user,
		int maxPages = 5)
	{
		var pageNumber = 1;
		AnilistPage page;
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
		} while (page.PageInfo.HasNextPage && pageNumber <= maxPages);
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
						pagesOfMediaIds.GetOrAdd(pageNumber + 1, _ => []).Add(id);
					}
					else
					{
						storage.Remove(id);
						yield return new(
							Media: mediaDict[id],
							Score: tally.AverageScore,
							ScoredPopularity: tally.ScoredCount,
							TotalPopularity: tally.ScoredCount + tally.UnscoredCount
						);
					}
				}
			} while (mediaIds.Count > 0);
		}
	}

	public static async IAsyncEnumerable<AnilistMedia> GetAnilistListAsync(
		this HttpClient http,
		string username,
		AnilistMediaListStatus status)
	{
		var alreadyReturned = new HashSet<int>();

		var chunk = 1;
		AnilistMediaListCollection collection;
		do
		{
			collection = await http.GetAnilistPlanningListChunkAsync(
				username: username,
				status: status,
				chunk: chunk
			).ConfigureAwait(false);

			foreach (var list in collection.Lists)
			{
				foreach (var entry in list.Entries)
				{
					// I think an entry can be returned multiple times if it's
					// in multiple custom lists and/or not hidden from the standard
					// planning list?
					if (alreadyReturned.Add(entry.Media.Id))
					{
						var personalScore = (int?)entry.Score is int i && i > 0 ? (int?)i : null;
						yield return entry.Media with
						{
							PersonalScore = personalScore,
							User = collection.User,
						};
					}
				}
			}

			++chunk;
		} while (collection.HasNextChunk);
	}

	private static async Task<AnilistPage> GetAnilistFollowersPageAsync(
		this HttpClient http,
		int userId,
		int page
	)
	{
		// Sort by Watched Time since I guess that's a better metric than user id?
		var query = $@"
		query ($userId: Int!, $page: Int) {{
			Page(page: $page, perPage: {PAGE_SIZE}) {{
				pageInfo {{
					hasNextPage
				}}
				following(userId: $userId, sort: WATCHED_TIME_DESC) {{
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
		AnilistMediaListStatus status,
		int chunk)
	{
		var query = $@"
		query ($username: String, $chunk: Int) {{
			MediaListCollection(userName: $username, type: ANIME, status: {status}, perChunk: {CHUNK_SIZE}, chunk: $chunk) {{
				hasNextChunk
				user {{
					id
				}}
				lists {{
					entries {{
						score(format: POINT_100)
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

		return (await http.GetResponseAsync<AnilistListQuery>(new
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
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

		if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var remaining))
		{
			var amount = int.Parse(remaining.Single());
#if DEBUG
			Console.WriteLine($"{DateTime.UtcNow:T}: Requests remaining {amount}");
#endif
			// not sure if the OPTIONS request due to cors counts as a 2nd request
			// against cloudflare but not against whatever is counting the ratelimit
			// header, or if there's some other issue
			// there's probably some better way to handle ratelimits, but this
			// isn't a huge issue with the way this app is structured
			if (amount < 85)
			{
				await Task.Delay(TimeSpan.FromSeconds(0.1)).ConfigureAwait(false);
			}
			if (amount < 70)
			{
				await Task.Delay(TimeSpan.FromSeconds(0.50)).ConfigureAwait(false);
			}
			if (amount < 60)
			{
				await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			}
		}
		if (response.Headers.TryGetValues("Retry-After", out var retry))
		{
			var seconds = int.Parse(retry.Single());
			await Task.Delay(TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
		}

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

public record struct AnilistFriendScore(
	AnilistMedia Media,
	int? Score,
	int ScoredPopularity,
	int TotalPopularity
);