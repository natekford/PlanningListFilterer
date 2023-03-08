using BlazorTest.Models.Anilist.Json;

using System.Collections.Immutable;

namespace BlazorTest.Models.Anilist.Search;

public sealed class AnilistSearch
{
	private readonly ImmutableArray<IAnilistSearchItem> _Items;
	private readonly IEnumerable<AnilistModel> _Media;

	public AnilistSearchMinMax Duration { get; }
	// has to be nullable otherwise multiselect only shows the 1st enum
	public AnilistSearchFormats Formats { get; }
	public AnilistSearchGenres Genres { get; }
	public AnilistSearchMinMax Popularity { get; }
	public AnilistSearchMinMax Score { get; }
	public AnilistSearchTags Tags { get; }
	public AnilistSearchMinMax Year { get; }

	public AnilistSearch(IEnumerable<AnilistModel> media)
	{
		Duration = new(this, x => x.GetTotalDuration());
		Formats = new(this);
		Genres = new(this);
		Popularity = new(this, x => x.Popularity);
		Score = new(this, x => x.AverageScore);
		Tags = new(this);
		Year = new(this, x => x.StartYear);

		_Media = media;
		_Items = GetType()
			.GetProperties()
			.Select(x => x.GetValue(this))
			.OfType<IAnilistSearchItem>()
			.ToImmutableArray();
	}

	public static async Task<AnilistSearch> CreateAsync(IEnumerable<AnilistModel> media)
	{
		var search = new AnilistSearch(media);
		await search.UpdateVisibilityAsync().ConfigureAwait(false);
		return search;
	}

	public Task Clear()
	{
		foreach (var item in _Items)
		{
			item.Clear();
		}
		return UpdateVisibilityAsync();
	}

	public async Task UpdateVisibilityAsync()
	{
		var formats = new HashSet<AnilistMediaFormat?>();
		var genres = new HashSet<string>();
		var tags = new HashSet<string>();

		foreach (var media in _Media)
		{
			// await so the UI is more responsive
			await Task.Yield();

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

			foreach (var genre in media.Genres)
			{
				genres.Add(genre);
			}
			foreach (var tag in media.Tags)
			{
				tags.Add(tag.Key);
			}
		}

		Formats.SetOptions(formats);
		Genres.SetOptions(genres);
		Tags.SetOptions(tags);
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