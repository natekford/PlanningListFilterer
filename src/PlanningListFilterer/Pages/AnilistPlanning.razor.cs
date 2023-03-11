using PlanningListFilterer.Models.Anilist;
using PlanningListFilterer.Models.Anilist.Json;
using PlanningListFilterer.Models.Anilist.Filter;

using MudBlazor;

using System.Diagnostics;
using PlanningListFilterer.Settings;

namespace PlanningListFilterer.Pages;

public partial class AnilistPlanning
{
	private static readonly Random Random = new();
	private int _RandomId;

	public List<AnilistModel> Entries { get; set; } = new();
	public DialogOptions FilterDialogOptions { get; set; } = new()
	{
		FullWidth = true,
		CloseOnEscapeKey = true,
		MaxWidth = MaxWidth.Medium,
		Position = DialogPosition.Center,
	};
	public AnilistFilterer Filterer { get; set; } = new(Enumerable.Empty<AnilistModel>());
	public MudDataGrid<AnilistModel> Grid { get; set; } = null!;
	public bool IsFilterDialogVisible { get; set; }
	public bool IsLoading { get; set; }
	public ListSettings ListSettings { get; set; } = new();
	public string? Username { get; set; } = "advorange";

	public async IAsyncEnumerable<FriendScore> GetFriendScores(
		AnilistUser user,
		IEnumerable<AnilistMedia> media)
	{
		if (!ListSettings.EnableFriendScores)
		{
			foreach (var m in media)
			{
				yield return new(m, null);
			}
			yield break;
		}

		var friends = new List<AnilistUser>();
		await foreach (var friend in Http.GetAnilistFollowingAsync(user).ConfigureAwait(false))
		{
			friends.Add(friend);
		}
		await foreach (var fScore in Http.GetAnilistFriendScoresAsync(media, friends).ConfigureAwait(false))
		{
			yield return fScore;
		}
	}

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
		var sw = Stopwatch.StartNew();

		var entries = default(List<AnilistModel>);
		if (useCached)
		{
			try
			{
				entries = await LocalStorage.GetItemCompressedAsync<List<AnilistModel>>(username.Name);
				Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved cached");
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

		var media = new List<AnilistMedia>();
		var user = default(AnilistUser);
		await foreach (var entry in Http.GetAnilistPlanningListAsync(username.Name).ConfigureAwait(false))
		{
			user ??= entry.User;
			if (entry.Media.Status == AnilistMediaStatus.FINISHED)
			{
				media.Add(entry.Media);
			}
		}
		Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved planning list");
		entries = new List<AnilistModel>(media.Count);

		await foreach (var fScore in GetFriendScores(user!, media).ConfigureAwait(false))
		{
			entries.Add(AnilistModel.Create(fScore.Media, fScore.Score));
		}
		Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved friend scores");
		entries.Sort((x, y) => x.Id.CompareTo(y.Id));

		await LocalStorage.SetItemCompressedAsync(username.Name, entries).ConfigureAwait(false);
		await LocalStorage.SetItemAsync(username.Meta, AnilistMeta.New(
			userId: user!.Id,
			savedWithFriendScores: ListSettings.EnableFriendScores
		)).ConfigureAwait(false);
		Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Saved uncached");

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
		var entries = await GetPlanningList(username, useCached).ConfigureAwait(false);

		Filterer = new(entries, StateHasChanged);
		await Filterer.UpdateVisibilityAsync().ConfigureAwait(false);
		Entries = entries;
		IsLoading = false;
	}

	public async Task RandomizeTable()
	{
		var visibleEntries = Entries.Where(x => x.IsVisible).ToList();
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

	public void ToggleFilterDialogVisibility()
		=> IsFilterDialogVisible = !IsFilterDialogVisible;

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