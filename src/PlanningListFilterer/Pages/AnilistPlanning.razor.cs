using Blazored.LocalStorage;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using PlanningListFilterer.Models.Anilist;
using PlanningListFilterer.Models.Anilist.Json;
using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Pages;

public partial class AnilistPlanning
{
	private const string LAST_USERNAME = "_LastSearchedUsername";

	private static readonly Random _Random = new();
	private readonly HashSet<int> _ShownRandomIds = [];

	public ColumnSettings ColumnSettings { get; set; } = new();
	public List<AnilistModel> Entries { get; set; } = [];
	public required Column<AnilistModel> GenreColumn { get; set; } = null!;
	public required MudDataGrid<AnilistModel> Grid { get; set; } = null!;
	[Inject]
	public required HttpClient Http { get; set; } = null!;
	public bool IsLoading { get; set; }
	public ListSettings ListSettings { get; set; } = new();
	[Inject]
	public required ILocalStorageService LocalStorage { get; set; } = null!;
	[Inject]
	public required ILogger<AnilistPlanning> Logger { get; set; } = null!;
	[Inject]
	public required SettingsService Settings { get; set; } = null!;
	public required Column<AnilistModel> TagColumn { get; set; } = null!;
	public string? Username { get; set; }

	public async Task<AnilistMeta?> GetMeta(Username username)
	{
		try
		{
			return await LocalStorage.GetItemAsync<AnilistMeta>(username.Meta).ConfigureAwait(false);
		}
		catch
		{
			Logger.LogWarning("Unable to load meta for {user}.", username.Name);
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
				entries = await LocalStorage.GetItemCompressedAsync<List<AnilistModel>>(username.Key);
			}
			catch
			{
				Logger.LogWarning("Unable to load saved list for {user}.", username.Name);
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
					FriendPopularityScored = x.ScoredPopularity,
					FriendPopularityTotal = x.TotalPopularity
				};
			});
		}
		else
		{
			entries = medias.ConvertAll(AnilistModel.Create);
		}

		entries.Sort((x, y) => x.Id.CompareTo(y.Id));
		await LocalStorage.SetItemCompressedAsync(username.Key, entries).ConfigureAwait(false);
		await LocalStorage.SetItemAsync(username.Meta, new AnilistMeta(
			userId: user!.Id,
			settings: ListSettings
		)).ConfigureAwait(false);
		await LocalStorage.SetItemAsync(LAST_USERNAME, username.Name).ConfigureAwait(false);

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
		Entries = [];

		var username = new Username(Username);
		var meta = await GetMeta(username).ConfigureAwait(false);
		var useCached = meta?.ShouldReacquire(ListSettings, TimeSpan.FromHours(1)) == false;

		try
		{
			Entries = await GetPlanningList(username, useCached).ConfigureAwait(false);
		}
		catch (HttpRequestException e)
		{
			Logger.LogError(e, "HTTP error");
		}
		catch (Exception e)
		{
			Logger.LogError(e, "Other error");
		}
		finally
		{
			IsLoading = false;
		}
	}

	public async Task RandomizeTable()
	{
		var visibleIds = Grid.FilteredItems.Select(x => x.Id);
		if (!visibleIds.Any())
		{
			return;
		}

		// Prevent showing duplicate random entries
		var validIds = visibleIds.Where(x => !_ShownRandomIds.Contains(x)).ToList();
		if (validIds.Count == 0)
		{
			// Only remove visible ids, otherwise if a user randomizes
			// 100+ entries in their 1000+ entry list, then filters down to
			// 2 entries, once those 2 entries have been shuffled through, the 100+
			// previously shown entries would be thrown in the pool of
			// valid entries, and people hate when randomize functions
			// show something repeatedly too soon
			_ShownRandomIds.ExceptWith(visibleIds);
			validIds = visibleIds.ToList();
		}

		var randomId = validIds[_Random.Next(0, validIds.Count)];
		_ShownRandomIds.Add(randomId);

		await Grid.SetSortAsync(
			field: "Random",
			direction: SortDirection.Ascending,
			sortFunc: x => x.Id <= randomId
		).ConfigureAwait(false);
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
		{
			return;
		}

		Username = await LocalStorage.GetItemAsync<string>(LAST_USERNAME).ConfigureAwait(false);
		ListSettings = await Settings.GetAsync<ListSettings>().ConfigureAwait(false);
		ColumnSettings = await Settings.GetAsync<ColumnSettings>().ConfigureAwait(false);

		foreach (var column in Grid.RenderedColumns)
		{
			if (column.Hideable != false && ColumnSettings.HiddenColumns.Contains(column.PropertyName))
			{
				await column.HideAsync().ConfigureAwait(false);
			}
		}

		StateHasChanged();
	}
}

public readonly struct Username
{
	public string Key { get; }
	public string Meta { get; }
	public string Name { get; }

	public Username(string username)
	{
		Name = username.ToLower();
		Key = $"LIST_{Name}";
		Meta = $"META_{Name}";
	}
}