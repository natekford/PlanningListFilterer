using BlazorTest.Models;

using System.Collections.Immutable;

namespace BlazorTest.ViewModels;

public sealed class SearchViewModel
{
	private readonly IEnumerable<Media> _Media;

	public ImmutableArray<string> AllowedGenres { get; private set; } = ImmutableArray<string>.Empty;
	public ImmutableArray<string> AllowedTags { get; private set; } = ImmutableArray<string>.Empty;
	public ImmutableHashSet<string> Genres { get; set; } = ImmutableHashSet<string>.Empty;
	public bool IsModalActive { get; private set; }
	public int? MaximumYear { get; private set; }
	public int? MinimumYear { get; private set; }
	public ImmutableHashSet<string> Tags { get; set; } = ImmutableHashSet<string>.Empty;

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
		Genres = Genres.Add(genre);
		return UpdateVisibilityAsync();
	}

	public Task AddTag(string tag)
	{
		Tags = Tags.Add(tag);
		return UpdateVisibilityAsync();
	}

	public Task RemoveGenre(string genre)
	{
		Genres = Genres.Remove(genre);
		return UpdateVisibilityAsync();
	}

	public Task RemoveTag(string tag)
	{
		Tags = Tags.Remove(tag);
		return UpdateVisibilityAsync();
	}

	public Task SetMaximumYear(int? maximum)
	{
		MaximumYear = maximum;
		return UpdateVisibilityAsync();
	}

	public Task SetMinimumYear(int? minimum)
	{
		MinimumYear = minimum;
		return UpdateVisibilityAsync();
	}

	public void ToggleModal()
		=> IsModalActive = !IsModalActive;

	public async Task UpdateVisibilityAsync()
	{
		var genres = new HashSet<string>();
		var tags = new HashSet<string>();

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
					genres.Add(genre);
				}
			}
			foreach (var tag in media.Tags)
			{
				if (!Tags.Contains(tag.Name))
				{
					tags.Add(tag.Name);
				}
			}

			// await so the UI is more responsive
			await Task.Yield();
		}

		AllowedGenres = genres.OrderBy(x => x).ToImmutableArray();
		AllowedTags = tags.OrderBy(x => x).ToImmutableArray();
	}

	private bool GetUpdatedVisibility(Media media)
	{
		var year = media.GetReleaseYear();
		if (MinimumYear.HasValue && year < MinimumYear)
		{
			return false;
		}
		if (MaximumYear.HasValue && year > MaximumYear)
		{
			return false;
		}
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