using BlazorTest.Models;

using MudBlazor;

namespace BlazorTest.Pages;

public partial class AnilistPlanning
{
	public List<AnilistMedia> Entries { get; set; } = new();
	public bool IsSearchVisible { get; set; }
	public MediaSearch Search { get; set; } = new(Enumerable.Empty<AnilistMedia>());
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
		var response = await LocalStorage.GetItemAsync<AnilistResponse>(key).ConfigureAwait(false);
		if (response is null
			|| (DateTime.UtcNow - response.ReceivedAt) > TimeSpan.FromMinutes(15))
		{
			response = await Http.GetAnilistAsync(Username).ConfigureAwait(false);
			await LocalStorage.SetItemAsync(key, response).ConfigureAwait(false);
		}

		Entries = response.Data.MediaListCollection.Lists
			.SelectMany(l => l.Entries.Select(e => e.Media))
			.Where(x => x?.Status == AnilistMediaStatus.FINISHED)
			.OrderBy(x => x.Id)
			.ToList();

		Search = await MediaSearch.CreateAsync(Entries).ConfigureAwait(false);
	}

	public void ToggleSearchVisibility()
		=> IsSearchVisible = !IsSearchVisible;
}