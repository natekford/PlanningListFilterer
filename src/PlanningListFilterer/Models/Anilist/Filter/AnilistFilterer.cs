using PlanningListFilterer.Models.Anilist.Json;

using System.Collections.Immutable;

namespace PlanningListFilterer.Models.Anilist.Filter;

public sealed class AnilistFilterer
{
	private readonly ImmutableArray<AnilistFilter> _Filters;
	private readonly IEnumerable<AnilistModel> _Media;
	private readonly Action? _OnVisibiltyUpdated;

	public AnilistFilterGenres Genres { get; }
	public AnilistFilterTags Tags { get; }

	public AnilistFilterer(
		IEnumerable<AnilistModel> media,
		Action? onVisibilityUpdated = null)
	{
		Genres = new(this);
		Tags = new(this);

		_Media = media;
		_OnVisibiltyUpdated = onVisibilityUpdated;
		_Filters = GetType()
			.GetProperties()
			.Select(x => x.GetValue(this))
			.OfType<AnilistFilter>()
			.ToImmutableArray();
	}

	public Task Clear()
	{
		foreach (var filter in _Filters)
		{
			filter.Reset();
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

			media.IsVisible = GetUpdatedVisibility(media);
			if (!media.IsVisible)
			{
				continue;
			}

			genres.UnionWith(media.Genres);
			tags.UnionWith(media.Tags.Keys);
		}

		Genres.SetOptions(genres);
		Tags.SetOptions(tags);

		_OnVisibiltyUpdated?.Invoke();
	}

	private bool GetUpdatedVisibility(AnilistModel model)
	{
		foreach (var filter in _Filters)
		{
			if (!filter.IsValid(model))
			{
				return false;
			}
		}
		return true;
	}
}