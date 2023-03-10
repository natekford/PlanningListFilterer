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
			entries = new List<AnilistModel>();
			await foreach (var entry in Http.GetAnilistAsync(username.Name))
			{
				if (entry.Status == AnilistMediaStatus.FINISHED)
				{
					entries.Add(AnilistModel.Create(entry));
				}
			}
			entries.Sort((x, y) => x.Id.CompareTo(y.Id));

			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved uncached");
			await LocalStorage.SetItemCompressedAsync(username.Name, entries).ConfigureAwait(false);
			await LocalStorage.SetItemAsync(username.Meta, AnilistMeta.New()).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Saved uncached");
		}

		return entries;
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
		var useCached = meta?.IsOutOfDate(TimeSpan.FromMinutes(15)) == false;
		var entries = await GetAnilist(username, useCached).ConfigureAwait(false);

		Search = new(entries, StateHasChanged);
		await Search.UpdateVisibilityAsync().ConfigureAwait(false);
		Entries = entries;
		IsLoading = false;
	}

	public void RandomizeTable()
	{
		var visibleEntries = Entries.Where(x => x.IsEntryVisible).ToList();
		// Don't show the same
		int randomId;
		do
		{
			randomId = visibleEntries[Random.Next(0, visibleEntries.Count)].Id;
		} while (randomId == RandomId);

		RandomId = randomId;
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