using BlazorTest.Models;

using System.Collections.Immutable;

namespace BlazorTest.ViewModels;

public sealed class SearchViewModel
{
	private readonly IEnumerable<Media> _Media;

	public ImmutableArray<string> AvailableGenres { get; private set; } = ImmutableArray<string>.Empty;
	public ImmutableArray<string> AvailableTags { get; private set; } = ImmutableArray<string>.Empty;
	public HashSet<string> Genres { get; set; } = new();
	public bool IsModalActive { get; private set; }
	public HashSet<string> Tags { get; set; } = new();

	public SearchViewModel(IEnumerable<Media> media)
	{
		_Media = media;
	}

	public static async Task<SearchViewModel> CreateAsync(IEnumerable<Media> media)
	{
		var vm = new SearchViewModel(media);
		await vm.UpdateVisibilityAsync().ConfigureAwait(false);
		return vm;
	}

	public Task AddGenre(string genre)
	{
		Genres.Add(genre);
		return UpdateVisibilityAsync();
	}

	public Task AddTag(string tag)
	{
		Tags.Add(tag);
		return UpdateVisibilityAsync();
	}

	public Task RemoveGenre(string genre)
	{
		Genres.Remove(genre);
		return UpdateVisibilityAsync();
	}

	public Task RemoveTag(string tag)
	{
		Tags.Remove(tag);
		return UpdateVisibilityAsync();
	}

	public void ToggleModal()
		=> IsModalActive = !IsModalActive;

	public async Task UpdateVisibilityAsync()
	{
		var availableGenres = new HashSet<string>();
		var availableTags = new HashSet<string>();

		foreach (var media in _Media)
		{
			media.IsEntryVisible = GetUpdatedVisibility(media);
			if (!media.IsEntryVisible)
			{
				continue;
			}

			foreach (var genre in media.Genres)
			{
				if (!Genres.Contains(genre))
				{
					availableGenres.Add(genre);
				}
			}
			foreach (var tag in media.Tags)
			{
				if (!Tags.Contains(tag.Name))
				{
					availableTags.Add(tag.Name);
				}
			}

			// await so the UI is more responsive
			await Task.Yield();
		}

		AvailableGenres = availableGenres.OrderBy(x => x).ToImmutableArray();
		AvailableTags = availableTags.OrderBy(x => x).ToImmutableArray();
	}

	private bool GetUpdatedVisibility(Media media)
	{
		foreach (var genre in Genres)
		{
			if (!media.Genres.Contains(genre))
			{
				return false;
			}
		}
		foreach (var tag in Tags)
		{
			if (!media.Tags.Any(x => x.Name == tag))
			{
				return false;
			}
		}
		return true;
	}
}