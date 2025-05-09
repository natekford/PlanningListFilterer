using Blazored.LocalStorage;

using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;

using PlanningListFilterer.Models.Anilist;
using PlanningListFilterer.Models.Anilist.Json;
using PlanningListFilterer.Settings;

using System.Globalization;

namespace PlanningListFilterer.Pages;

public partial class AnilistPlanning
{
	private const string LAST_USERNAME = "_LastSearchedUsername";
	private const string ROWS_PER_PAGE = "_RowsPerPage";

	private static readonly Random _Random = new();
	private readonly HashSet<int> _ShownRandomIds = [];

	public AnilistMetaCollection AnilistMeta { get; set; } = [];
	public ColumnSettings ColumnSettings { get; set; } = new();
	public List<AnilistModel> Entries { get; set; } = [];
	public required Column<AnilistModel> GenreColumn { get; set; }
	public required MudDataGrid<AnilistModel> Grid { get; set; }
	[Inject]
	public required HttpClient Http { get; set; }
	public bool IsLoading { get; set; }
	[Inject]
	public required IJSRuntime Js { get; set; }
	public ListSettings ListSettings { get; set; } = new();
	[Inject]
	public required ILocalStorageService LocalStorage { get; set; }
	[Inject]
	public required ILogger<AnilistPlanning> Logger { get; set; }
	[Inject]
	public required IPopoverService Popover { get; set; }
	[Inject]
	public required SettingsService Settings { get; set; }
	public required Column<AnilistModel> TagColumn { get; set; }
	public int TagPercent { get; set; }
	public AnilistUsername Username { get; set; } = new("");

	public async Task DownloadAsCsv()
	{
		if (Entries.Count == 0)
		{
			return;
		}

		await using var ms = new MemoryStream();
		using var sRef = new DotNetStreamReference(ms);

		await using (var sw = new StreamWriter(ms, leaveOpen: true))
		await using (var csv = new CsvWriter(sw, new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			InjectionOptions = InjectionOptions.Escape,
		}))
		{
			csv.Context.RegisterClassMap<AnilistModelMap>();
			csv.WriteRecords(Entries);
		}
		ms.Position = 0;

		var time = DateTime.UtcNow.ToString("s").Replace(":", ".");
		var fileName = $"Planning_{time}.csv";

		await Js.InvokeVoidAsync("downloadFileFromStream", fileName, sRef);
	}

	public async Task<List<AnilistModel>> GetPlanningList(AnilistUsername username, bool useCached)
	{
		var listKey = username.GetListKey(ListSettings.ListStatus);
		var entries = default(List<AnilistModel>);
		if (useCached)
		{
			try
			{
				entries = await LocalStorage.GetItemCompressedAsync<List<AnilistModel>>(
					key: listKey
				).ConfigureAwait(false);
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
		await foreach (var entry in Http.GetAnilistListAsync(username.Name, ListSettings.ListStatus).ConfigureAwait(false))
		{
			user ??= entry.User;
			if (entry.Status is AnilistMediaStatus.FINISHED
				or AnilistMediaStatus.RELEASING
				or AnilistMediaStatus.CANCELLED)
			{
				medias.Add(entry);
			}
		}
		if (user is null)
		{
			throw new InvalidOperationException(
				$"User ({username.Name}) was null when it should not be possible.");
		}

		// Don't bother getting friends if there is no media to get scores of
		if (medias.Count > 0 && ListSettings.EnableFriendScores)
		{
			var friends = new List<AnilistUser>();
			await foreach (var friend in Http.GetAnilistFollowingAsync(user).ConfigureAwait(false))
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
		var length = await LocalStorage.SetItemCompressedAsync(
			key: listKey,
			data: entries
		).ConfigureAwait(false);
		await LocalStorage.SetItemAsync(LAST_USERNAME, username.Name).ConfigureAwait(false);

		AnilistMeta[listKey] = new AnilistMeta(
			userId: user.Id,
			length: length,
			settings: ListSettings
		);
		if (AnilistMeta.Count > 25)
		{
			var keys = AnilistMeta
				.OrderBy(x => x.Value.SavedAt)
				.Take(5)
				.Select(x => x.Key)
				.ToList();
			foreach (var key in keys)
			{
				await LocalStorage.RemoveItemAsync(key).ConfigureAwait(false);
				AnilistMeta.Remove(key);
			}
		}
		await Settings.SaveAsync(AnilistMeta).ConfigureAwait(false);

		return entries;
	}

	public async Task LoadEntries()
	{
		if (!Username.IsValid)
		{
			return;
		}

		IsLoading = true;
		// Stop showing old entries
		Entries = [];

		var useCached = false;
		if (AnilistMeta.TryGetValue(Username.GetListKey(ListSettings.ListStatus), out var meta))
		{
			useCached = !meta.ShouldReacquire(ListSettings, TimeSpan.FromHours(1));
		}

		try
		{
			Entries = await GetPlanningList(Username, useCached).ConfigureAwait(false);
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

	public void SaveRowsPerPage()
	{
		InvokeAsync(async () =>
		{
			try
			{
				await LocalStorage.SetItemAsync(ROWS_PER_PAGE, Grid.RowsPerPage).ConfigureAwait(false);
			}
			catch
			{
				Logger.LogWarning("Unable to save rows per page for table.");
			}
		});
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
		{
			return;
		}

		Username = new(await LocalStorage.GetItemAsync<string>(LAST_USERNAME).ConfigureAwait(false));
		AnilistMeta = await Settings.GetAsync<AnilistMetaCollection>().ConfigureAwait(false) ?? [];
		ListSettings = await Settings.GetAsync<ListSettings>().ConfigureAwait(false);
		ColumnSettings = await Settings.GetAsync<ColumnSettings>().ConfigureAwait(false);

		await ColumnSettings.SetVisibilityAsync(
			columns: Grid.RenderedColumns,
			properties: ColumnSettings.HiddenColumns,
			visible: false
		).ConfigureAwait(false);

		var rowsPerPage = await LocalStorage.GetItemAsync<int?>(ROWS_PER_PAGE).ConfigureAwait(false) ?? 100;
		await Grid.SetRowsPerPageAsync(rowsPerPage).ConfigureAwait(false);
		Grid.PagerStateHasChangedEvent += SaveRowsPerPage;

		StateHasChanged();
	}
}