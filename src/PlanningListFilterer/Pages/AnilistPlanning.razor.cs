using MudBlazor;

using PlanningListFilterer.Models.Anilist;
using PlanningListFilterer.Models.Anilist.Json;
using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Pages;

public partial class AnilistPlanning
{
	private static readonly Random Random = new();
	private int _RandomId;

	public List<AnilistModel> Entries { get; set; } = new();
	public IEnumerable<AnilistModel> FilteredEntries => Grid.FilteredItems;
	public bool IsLoading { get; set; }
	public ListSettings ListSettings { get; set; } = new();
	public string? Username { get; set; } = "advorange";

	public async Task<AnilistMeta?> GetMeta(Username username)
	{
		try
		{
			return await LocalStorage.GetItemAsync<AnilistMeta>(username.Meta).ConfigureAwait(false);
		}
		catch
		{
			return null;
		}
	}

	public async Task<List<AnilistModel>> GetPlanningList(Username username, bool useCached)
	{
		var entries = default(List<AnilistModel>);
		if (useCached)
		{
			try
			{
				entries = await LocalStorage.GetItemCompressedAsync<List<AnilistModel>>(username.Name);
			}
			catch
			{
				// json error or something, nothing we can do to save this
				// retrieve new list from anilist
			}
		}
		if (entries is not null)
		{
			return entries;
		}

		var medias = new List<AnilistMedia>();
		var user = default(AnilistUser);
		await foreach (var entry in Http.GetAnilistPlanningListAsync(username.Name).ConfigureAwait(false))
		{
			user ??= entry.User;
			if (entry.Media.Status == AnilistMediaStatus.FINISHED)
			{
				medias.Add(entry.Media);
			}
		}

		if (ListSettings.EnableFriendScores)
		{
			var friends = new List<AnilistUser>();
			await foreach (var friend in Http.GetAnilistFollowingAsync(user!).ConfigureAwait(false))
			{
				friends.Add(friend);
			}
			var friendScores = new List<AnilistFriendScore>(medias.Count);
			await foreach (var friendScore in Http.GetAnilistFriendScoresAsync(medias, friends).ConfigureAwait(false))
			{
				friendScores.Add(friendScore);
			}

			entries = friendScores.ConvertAll(x =>
			{
				return AnilistModel.Create(x.Media) with
				{
					FriendScore = x.Score,
					FriendPopularity = x.Popularity,
				};
			});
		}
		else
		{
			entries = medias.ConvertAll(AnilistModel.Create);
		}

		entries.Sort((x, y) => x.Id.CompareTo(y.Id));
		await LocalStorage.SetItemCompressedAsync(username.Name, entries).ConfigureAwait(false);
		await LocalStorage.SetItemAsync(username.Meta, AnilistMeta.New(
			userId: user!.Id,
			savedWithFriendScores: ListSettings.EnableFriendScores
		)).ConfigureAwait(false);

		return entries;
	}

	public async Task LoadEntries()
	{
		if (Username is null)
		{
			return;
		}

		IsLoading = true;
		// Stop showing old entries
		Entries = new List<AnilistModel>();

		var username = new Username(Username);
		var meta = await GetMeta(username).ConfigureAwait(false);
		var useCached = meta?.ShouldReacquire(ListSettings, TimeSpan.FromHours(1)) == false;

		Entries = await GetPlanningList(username, useCached).ConfigureAwait(false);
		IsLoading = false;
	}

	public async Task RandomizeTable()
	{
		var visibleEntries = Grid.FilteredItems.ToList();
		// Don't show the same
		int randomId;
		do
		{
			randomId = visibleEntries[Random.Next(0, visibleEntries.Count)].Id;
		} while (randomId == _RandomId);

		_RandomId = randomId;
		await Grid.SetSortAsync(
			field: "Random",
			direction: SortDirection.Ascending,
			sortFunc: x => x.Id <= _RandomId
		).ConfigureAwait(false);
	}

	protected override async Task OnInitializedAsync()
		=> ListSettings = await ListSettingsService.GetSettingsAsync().ConfigureAwait(false);
}

public sealed class Username
{
	public string Meta { get; }
	public string Name { get; }

	public Username(string username)
	{
		Name = username.ToLower();
		Meta = $"{Name}-META";
	}
}