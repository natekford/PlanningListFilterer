using BlazorTest.Models;

using MudBlazor;

using System.Diagnostics;

namespace BlazorTest.Pages;

public partial class AnilistPlanning
{
	private static readonly TimeSpan ListTimeout = TimeSpan.FromMinutes(15);

	public List<AnilistViewModel> Entries { get; set; } = new();
	public bool IsSearchVisible { get; set; }
	public MediaSearch Search { get; set; } = new(Enumerable.Empty<AnilistViewModel>());
	public DialogOptions SearchDialogOptions { get; set; } = new()
	{
		FullWidth = true,
		CloseOnEscapeKey = true,
		MaxWidth = MaxWidth.Large,
		Position = DialogPosition.Center,
	};
	public string? Username { get; set; } = "advorange";

	public async Task LoadEntries()
	{
		if (Username is null)
		{
			return;
		}

		var sw = Stopwatch.StartNew();

		var key = Username.ToLower();
		var list = default(AnilistViewModelList);
		try
		{
			list = await LocalStorage.GetItemAsync<AnilistViewModelList>(key).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved Cached");
		}
		catch
		{
		}

		if (list is null || (DateTime.UtcNow - list.SavedAt) > ListTimeout)
		{
			var response = await Http.GetAnilistAsync(Username).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Retrieved External");
			var entries = response.Data.MediaListCollection.Lists
				.SelectMany(l => l.Entries.Select(e => AnilistViewModel.FromMedia(e.Media)))
				.Where(x => x?.Status == AnilistMediaStatus.FINISHED)
				.OrderBy(x => x.Id)
				.ToList();
			list = new(entries, DateTime.UtcNow);

			await LocalStorage.SetItemAsync(key, list).ConfigureAwait(false);
			Console.WriteLine($"{sw.ElapsedMilliseconds}ms: Saved");
		}

		Entries = list.Entries;
		Search = await MediaSearch.CreateAsync(Entries).ConfigureAwait(false);
	}

	public void ToggleSearchVisibility()
		=> IsSearchVisible = !IsSearchVisible;
}