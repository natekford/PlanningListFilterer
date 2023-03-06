using BlazorTest.Models.Anilist;
using BlazorTest.Models.Anilist.Json;

using MudBlazor;

using System.Diagnostics;

namespace BlazorTest.Pages;

public partial class AnilistPlanning
{
	private static readonly TimeSpan ListTimeout = TimeSpan.FromMinutes(15);
	private static readonly Random Random = new();

	public List<AnilistModel> Entries { get; set; } = new();
	public bool IsSearchVisible { get; set; }
	public int RandomId { get; private set; }
	public MudTableSortLabel<AnilistModel> RandomSort { get; }
	public AnilistSearch Search { get; set; } = new(Enumerable.Empty<AnilistModel>());
	public DialogOptions SearchDialogOptions { get; set; } = new()
	{
		FullWidth = true,
		CloseOnEscapeKey = true,
		MaxWidth = MaxWidth.Large,
		Position = DialogPosition.Center,
	};
	public MudTable<AnilistModel> Table { get; set; } = null!;
	public string? Username { get; set; } = "advorange";

	public AnilistPlanning()
	{
		RandomSort = new()
		{
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
			SortDirection = SortDirection.Ascending,
			SortBy = x => x.Id <= RandomId,
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
		};
	}

	public async Task LoadEntries()
	{
		if (Username is null)
		{
			return;
		}

		var sw = Stopwatch.StartNew();

		var key = Username.ToLower();
		var list = default(AnilistCollection);
		try
		{
			list = await LocalStorage.GetItemAsync<AnilistCollection>(key).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved Cached");
		}
		catch (Exception e)
		{
			Console.WriteLine($"Exception when retrieving cached list for {key}:\n{e}");
			await LocalStorage.RemoveItemAsync(key).ConfigureAwait(false);
		}

		if (list is null || (DateTime.UtcNow - list.SavedAt) > ListTimeout)
		{
			var response = await Http.GetAnilistAsync(Username).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved External");
			var entries = response.Data.MediaListCollection.Lists
				.SelectMany(l => l.Entries.Select(e => AnilistModel.Create(e.Media)))
				.Where(x => x?.Status == AnilistMediaStatus.FINISHED)
				.OrderBy(x => x.Id)
				.ToList();
			list = new(entries, DateTime.UtcNow);

			await LocalStorage.SetItemAsync(key, list).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Saved");
		}

		Entries = list.Entries;
		Search = await AnilistSearch.CreateAsync(Entries).ConfigureAwait(false);
	}

	public void RandomizeTable()
	{
		var visibleEntries = Entries.Where(x => x.IsEntryVisible).ToList();
		int id;
		do
		{
			id = visibleEntries[Random.Next(0, visibleEntries.Count)].Id;
		} while (id == RandomId);
		RandomId = id;
		Table.Context.SetSortFunc(RandomSort);
	}

	public void ToggleSearchVisibility()
		=> IsSearchVisible = !IsSearchVisible;
}