using BlazorTest.Models;

using MudBlazor;

using System.Diagnostics;

namespace BlazorTest.Pages;

public partial class AnilistPlanning
{
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

		var key = Username.ToLower();
		var list = await LocalStorage.GetItemAsync<AnilistViewModelList>(key).ConfigureAwait(false);
		if (list is null
			|| (DateTime.UtcNow - list.SavedAt) > TimeSpan.FromMinutes(15))
		{
			var response = await Http.GetAnilistAsync(Username).ConfigureAwait(false);
			var entries = response.Data.MediaListCollection.Lists
				.SelectMany(l => l.Entries.Select(e => AnilistViewModel.FromMedia(e.Media)))
				.Where(x => x?.Status == AnilistMediaStatus.FINISHED)
				.OrderBy(x => x.Id)
				.ToList();
			list = new(entries, DateTime.UtcNow);

			await LocalStorage.SetItemAsync(key, list).ConfigureAwait(false);
		}

		Entries = list.Entries;
		Search = await MediaSearch.CreateAsync(Entries).ConfigureAwait(false);
	}

	public void ToggleSearchVisibility()
		=> IsSearchVisible = !IsSearchVisible;
}