using BlazorTest.Models.Anilist;
using BlazorTest.Models.Anilist.Json;
using BlazorTest.Models.Anilist.Search;

using MudBlazor;

using System.Diagnostics;

namespace BlazorTest.Pages;

public partial class AnilistPlanning
{
	private static readonly Random Random = new();

	public List<AnilistModel> Entries { get; set; } = new();
	public bool IsLoading { get; set; }
	public bool IsSearchVisible { get; set; }
	public int RandomId { get; set; }
	public AnilistSearch Search { get; set; } = new(Enumerable.Empty<AnilistModel>());
	public DialogOptions SearchDialogOptions { get; set; } = new()
	{
		FullWidth = true,
		CloseOnEscapeKey = true,
		MaxWidth = MaxWidth.Medium,
		Position = DialogPosition.Center,
	};
	public MudTable<AnilistModel> Table { get; set; } = null!;
	public string? Username { get; set; } = "advorange";

	public async Task<List<AnilistModel>> GetAnilist(Username username, bool useCached)
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

		if (entries is null)
		{
			var response = await Http.GetAnilistAsync(username.Name).ConfigureAwait(false);
			entries = response.Data.MediaListCollection.Lists
				.SelectMany(l => l.Entries.Select(e => AnilistModel.Create(e.Media)))
				.Where(x => x.Status == AnilistMediaStatus.FINISHED)
				.OrderBy(x => x.Id)
				.ToList();
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved uncached");
			await LocalStorage.SetItemCompressedAsync(username.Name, entries).ConfigureAwait(false);
			await LocalStorage.SetItemAsync(username.Meta, AnilistMeta.New()).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Saved uncached");
		}

		return entries;
	}

	public async Task LoadEntries()
	{
		if (Username is null)
		{
			return;
		}

		// stop showing old entries
		Entries = new List<AnilistModel>();
		IsLoading = true;

		var username = new Username(Username);
		var meta = default(AnilistMeta);
		try
		{
			meta = await LocalStorage.GetItemAsync<AnilistMeta>(username.Meta).ConfigureAwait(false);
		}
		catch
		{
		}

		var useCached = meta?.IsOutOfDate(TimeSpan.FromMinutes(15)) == false;
		var entries = await GetAnilist(username, useCached).ConfigureAwait(false);

		Search = await AnilistSearch.CreateAsync(entries).ConfigureAwait(false);
		Entries = entries;
		IsLoading = false;
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
		Table.Context.SetSortFunc(new()
		{
#pragma warning disable BL0005 // Component parameter should not be set outside of its component.
			SortDirection = SortDirection.Ascending,
			SortBy = x => x.Id <= RandomId,
#pragma warning restore BL0005 // Component parameter should not be set outside of its component.
		});
	}

	public void ToggleSearchVisibility()
		=> IsSearchVisible = !IsSearchVisible;
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