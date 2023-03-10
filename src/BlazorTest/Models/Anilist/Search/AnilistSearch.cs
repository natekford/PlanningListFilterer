using BlazorTest.Models.Anilist.Json;

using System.Collections.Immutable;

namespace BlazorTest.Models.Anilist.Search;

public sealed class AnilistSearch
{
	private readonly ImmutableArray<AnilistSearchItem> _Items;
	private readonly IEnumerable<AnilistModel> _Media;
	private readonly Action? _OnSearchVisibiltyUpdated;

	public AnilistSearchMinMax Duration { get; }
	public AnilistSearchFormats Formats { get; }
	public AnilistSearchGenres Genres { get; }
	public AnilistSearchMinMax Popularity { get; }
	public AnilistSearchMinMax Score { get; }
	public AnilistSearchSequels Sequel { get; }
	public AnilistSearchTags Tags { get; }
	public AnilistSearchMinMax Year { get; }

	public AnilistSearch(
		IEnumerable<AnilistModel> media,
		Action? onSearchVisibilityUpdated = null)
	{
		Duration = new(this, x => x.GetTotalDuration());
		Formats = new(this);
		Genres = new(this);
		Popularity = new(this, x => x.Popularity);
		Score = new(this, x => x.AverageScore);
		Sequel = new(this);
		Tags = new(this);
		Year = new(this, x => x.Start.Year);

		_Media = media;
		_OnSearchVisibiltyUpdated = onSearchVisibilityUpdated;
		_Items = GetType()
			.GetProperties()
			.Select(x => x.GetValue(this))
			.OfType<AnilistSearchItem>()
			.ToImmutableArray();
	}

	public Task Clear()
	{
		foreach (var item in _Items)
		{
			item.Reset();
		}
		return UpdateVisibilityAsync();
	}

	public async Task UpdateVisibilityAsync()
	{
		var formats = new HashSet<AnilistMediaFormat?>();
		var genres = new HashSet<string>();
		var tags = new HashSet<string>();

		var i = 0;
		foreach (var media in _Media)
		{
			if (++i % 50 == 0)
			{
				// await so the UI is more responsive
				await Task.Delay(1).ConfigureAwait(false);
			}

			// formats are ORed instead of ANDed
			if (media.Format is AnilistMediaFormat format)
			{
				formats.Add(format);
			}

			media.IsEntryVisible = GetUpdatedVisibility(media);
			if (!media.IsEntryVisible)
			{
				continue;
			}

			genres.UnionWith(media.Genres);
			tags.UnionWith(media.Tags.Keys);
		}

		Formats.SetOptions(formats);
		Genres.SetOptions(genres);
		Tags.SetOptions(tags);

		_OnSearchVisibiltyUpdated?.Invoke();
	}

	private bool GetUpdatedVisibility(AnilistModel model)
	{
		foreach (var item in _Items)
		{
			if (!item.IsValid(model))
			{
				return false;
			}
		}
		return true;
	}
}