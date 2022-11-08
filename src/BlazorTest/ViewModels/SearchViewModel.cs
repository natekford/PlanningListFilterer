using BlazorTest.Models;

using Microsoft.AspNetCore.Components;

using System.Collections.Immutable;

namespace BlazorTest.ViewModels;

public sealed class SearchViewModel
{
	private readonly IEnumerable<Media> _Entries;

	public ImmutableArray<string> AvailableGenres { get; private set; }
	public ImmutableArray<string> AvailableTags { get; private set; }
	public HashSet<string> Genres { get; set; } = new();
	public bool IsModalActive { get; private set; }
	public HashSet<string> Tags { get; set; } = new();

	public SearchViewModel(IEnumerable<Media> entries)
	{
		_Entries = entries;
		UpdateAvailableSearchOptions();
	}

	public void GenreRemoved(string genre)
	{
		Genres.Remove(genre);
		UpdateVisibility();
	}

	public void GenreSelected(ChangeEventArgs e)
	{
		Genres.Add(e.Value?.ToString()!);
		UpdateVisibility();
	}

	public void TagRemoved(string tag)
	{
		Tags.Remove(tag);
		UpdateVisibility();
	}

	public void TagSelected(ChangeEventArgs e)
	{
		Tags.Add(e.Value?.ToString()!);
		UpdateVisibility();
	}

	public void ToggleModal()
		=> IsModalActive = !IsModalActive;

	public void UpdateVisibility()
	{
		foreach (var item in _Entries)
		{
			item.IsEntryVisible = GetUpdatedVisibility(item);
		}
		UpdateAvailableSearchOptions();
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

	private void UpdateAvailableSearchOptions()
	{
		var availableGenres = new HashSet<string>();
		var availableTags = new HashSet<string>();
		foreach (var entry in _Entries)
		{
			if (!entry.IsEntryVisible)
			{
				continue;
			}

			foreach (var genre in entry.Genres)
			{
				if (!Genres.Contains(genre))
				{
					availableGenres.Add(genre);
				}
			}
			foreach (var tag in entry.Tags)
			{
				if (!Tags.Contains(tag.Name))
				{
					availableTags.Add(tag.Name);
				}
			}
		}
		AvailableGenres = availableGenres.OrderBy(x => x).ToImmutableArray();
		AvailableTags = availableTags.OrderBy(x => x).ToImmutableArray();
	}
}