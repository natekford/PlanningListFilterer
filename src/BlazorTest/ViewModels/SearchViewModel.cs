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
		UpdateVisibility();
	}

	public void AddGenre(string genre)
	{
		Genres.Add(genre);
		UpdateVisibility();
	}

	public void AddTag(string tag)
	{
		Tags.Add(tag);
		UpdateVisibility();
	}

	public void RemoveGenre(string genre)
	{
		Genres.Remove(genre);
		UpdateVisibility();
	}

	public void RemoveTag(string tag)
	{
		Tags.Remove(tag);
		UpdateVisibility();
	}

	public void ToggleModal()
		=> IsModalActive = !IsModalActive;

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

	private void UpdateVisibility()
	{
		var availableGenres = new HashSet<string>();
		var availableTags = new HashSet<string>();

		foreach (var item in _Entries)
		{
			item.IsEntryVisible = GetUpdatedVisibility(item);
			if (!item.IsEntryVisible)
			{
				continue;
			}

			foreach (var genre in item.Genres)
			{
				if (!Genres.Contains(genre))
				{
					availableGenres.Add(genre);
				}
			}
			foreach (var tag in item.Tags)
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