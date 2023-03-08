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

	public async Task<List<AnilistModel>> GetAnilist(string username, bool useCached)
	{
		var entries = default(List<AnilistModel>);
		if (useCached)
		{
			try
			{
				entries = await LocalStorage.GetItemAsync<List<AnilistModel>>(username);
			}
			catch
			{
				// json error or something, nothing we can do to save this
				// try to receive new entry information from anilist
			}
		}

		if (entries is null)
		{
			var response = await Http.GetAnilistAsync(username).ConfigureAwait(false);
			entries = response.Data.MediaListCollection.Lists
				.SelectMany(l => l.Entries.Select(e => AnilistModel.Create(e.Media)))
				.Where(x => x.Status == AnilistMediaStatus.FINISHED)
				.OrderBy(x => x.Id)
				.ToList();
			await LocalStorage.SetItemAsync(username, entries).ConfigureAwait(false);
		}

		return entries;
	}

	public async Task LoadEntries()
	{
		if (Username is null)
		{
			return;
		}

		var username = Username.ToLower();
		var metaKey = $"{username}-META";

		var meta = default(AnilistMeta);
		try
		{
			meta = await LocalStorage.GetItemAsync<AnilistMeta>(metaKey).ConfigureAwait(false);
		}
		catch
		{
		}

		var useCached = meta?.IsOutOfDate(TimeSpan.FromMinutes(15)) == false;
		var entries = await GetAnilist(username, useCached).ConfigureAwait(false);
		if (meta is null)
		{
			await LocalStorage.SetItemAsync(metaKey, AnilistMeta.Create(entries)).ConfigureAwait(false);
		}

		Entries = entries;
		Search = await AnilistSearch.CreateAsync(entries).ConfigureAwait(false);
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