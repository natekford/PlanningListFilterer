using System.Collections.Immutable;

namespace BlazorTest.Models;

public sealed class MediaSearch
{
	private readonly IEnumerable<AnilistMedia> _Media;

	public ImmutableArray<string> AllowedFormats { get; private set; } = ImmutableArray<string>.Empty;
	public ImmutableArray<string> AllowedGenres { get; private set; } = ImmutableArray<string>.Empty;
	public ImmutableArray<string> AllowedTags { get; private set; } = ImmutableArray<string>.Empty;
	public ImmutableHashSet<string> Formats { get; private set; } = ImmutableHashSet<string>.Empty;
	public ImmutableHashSet<string> Genres { get; private set; } = ImmutableHashSet<string>.Empty;
	public int? MaximumDuration { get; private set; }
	public int? MaximumYear { get; private set; }
	public int? MinimumDuration { get; private set; }
	public int? MinimumYear { get; private set; }
	public ImmutableHashSet<string> Tags { get; private set; } = ImmutableHashSet<string>.Empty;

	public MediaSearch(IEnumerable<AnilistMedia> media)
	{
		_Media = media;
	}

	public static async Task<MediaSearch> CreateAsync(IEnumerable<AnilistMedia> media)
	{
		var vm = new MediaSearch(media);
		await vm.UpdateVisibilityAsync().ConfigureAwait(false);
		return vm;
	}

	public Task Clear()
	{
		Formats = ImmutableHashSet<string>.Empty;
		Genres = ImmutableHashSet<string>.Empty;
		Tags = ImmutableHashSet<string>.Empty;
		MaximumDuration = null;
		MinimumDuration = null;
		MaximumYear = null;
		MinimumYear = null;
		return UpdateVisibilityAsync();
	}

	public Task SetFormats(IEnumerable<string> formats)
	{
		Formats = formats.ToImmutableHashSet();
		return UpdateVisibilityAsync();
	}

	public Task SetGenres(IEnumerable<string> genres)
	{
		Genres = genres.ToImmutableHashSet();
		return UpdateVisibilityAsync();
	}

	public Task SetMaximumDuration(int? max)
	{
		MaximumDuration = max;
		return UpdateVisibilityAsync();
	}

	public Task SetMaximumYear(int? max)
	{
		MaximumYear = max;
		return UpdateVisibilityAsync();
	}

	public Task SetMinimumDuration(int? min)
	{
		MinimumDuration = min;
		return UpdateVisibilityAsync();
	}

	public Task SetMinimumYear(int? min)
	{
		MinimumYear = min;
		return UpdateVisibilityAsync();
	}

	public Task SetTags(IEnumerable<string> tags)
	{
		Tags = tags.ToImmutableHashSet();
		return UpdateVisibilityAsync();
	}

	private bool GetUpdatedVisibility(AnilistMedia media)
	{
		if (media.StartDate?.Year is int year
			&& (year < MinimumYear || year > MaximumYear))
		{
			return false;
		}
		if (media.GetTotalDuration() is int duration
			&& (duration < MinimumDuration || duration > MaximumDuration))
		{
			return false;
		}
		if (media.Format is AnilistMediaFormat format
			&& Formats.Count > 0 && !Formats.Contains(format.ToString()))
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

	private async Task UpdateVisibilityAsync()
	{
		var formats = new HashSet<string>();
		var genres = new HashSet<string>();
		var tags = new HashSet<string>();

		foreach (var media in _Media)
		{
			// visbility doesn't matter for formats
			if (media.Format is AnilistMediaFormat format)
			{
				formats.Add(format.ToString());
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
				tags.Add(tag.Name);
			}

			// await so the UI is more responsive
			await Task.Yield();
		}

		AllowedFormats = formats
			.OrderByDescending(Formats.Contains)
			.ThenBy(x => x)
			.ToImmutableArray();
		AllowedGenres = genres
			.OrderByDescending(Genres.Contains)
			.ThenBy(x => x)
			.ToImmutableArray();
		AllowedTags = tags
			.OrderByDescending(Tags.Contains)
			.ThenBy(x => x)
			.ToImmutableArray();
	}
}