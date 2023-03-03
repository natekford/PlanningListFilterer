using BlazorTest.Models;

using System.Collections.Immutable;
using System.Net.Http.Json;

namespace BlazorTest.Pages;

public partial class AnilistPlanning
{
	public List<AnilistMedia> Entries { get; set; } = new();
	public MediaSearch Search { get; set; } = new(Enumerable.Empty<AnilistMedia>());
	public string? Username { get; set; }

	public async Task LoadEntries()
	{
#if false
		var response = await AnilistResponse.GetAnilistResponseAsync(Http, username!).ConfigureAwait(false);
#else
		var response = (await Http.GetFromJsonAsync<AnilistResponse>(
			requestUri: "sample-data/anilistresponse.json?a=1",
			options: AnilistResponse.JsonOptions
		).ConfigureAwait(false))!;
#endif

		Entries = response.Data.MediaListCollection.Lists
			.SelectMany(l => l.Entries.Select(e => e.Media))
			.Where(x => x?.Status == AnilistMediaStatus.FINISHED)
			.OrderBy(x => x.Id)
			.ToList();

		Search = await MediaSearch.CreateAsync(Entries).ConfigureAwait(false);
	}

	public sealed class MediaSearch
	{
		private readonly IEnumerable<AnilistMedia> _Media;

		public ImmutableArray<AnilistMediaFormat> AllowedFormats { get; private set; } = ImmutableArray<AnilistMediaFormat>.Empty;
		public ImmutableArray<string> AllowedGenres { get; private set; } = ImmutableArray<string>.Empty;
		public ImmutableArray<string> AllowedTags { get; private set; } = ImmutableArray<string>.Empty;
		public ImmutableHashSet<AnilistMediaFormat> Formats { get; set; } = ImmutableHashSet<AnilistMediaFormat>.Empty;
		public ImmutableHashSet<string> Genres { get; set; } = ImmutableHashSet<string>.Empty;
		public bool IsModalActive { get; private set; }
		public int? MaximumDuration { get; private set; }
		public int? MaximumYear { get; private set; }
		public int? MinimumDuration { get; private set; }
		public int? MinimumYear { get; private set; }
		public ImmutableHashSet<string> Tags { get; set; } = ImmutableHashSet<string>.Empty;

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
			Formats = ImmutableHashSet<AnilistMediaFormat>.Empty;
			Genres = ImmutableHashSet<string>.Empty;
			Tags = ImmutableHashSet<string>.Empty;
			MaximumDuration = null;
			MinimumDuration = null;
			MaximumYear = null;
			MinimumYear = null;
			return UpdateVisibilityAsync();
		}

		public Task SetFormats(IEnumerable<AnilistMediaFormat> formats)
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

		public void ToggleModal()
			=> IsModalActive = !IsModalActive;

		public async Task UpdateVisibilityAsync()
		{
			var formats = new HashSet<AnilistMediaFormat>();
			var genres = new HashSet<string>();
			var tags = new HashSet<string>();

			foreach (var media in _Media)
			{
				// visbility doesn't matter for formats
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
					tags.Add(tag.Name);
				}

				// await so the UI is more responsive
				await Task.Yield();
			}

			AllowedFormats = formats.OrderBy(x => x).ToImmutableArray();
			AllowedGenres = genres.OrderBy(x => x).ToImmutableArray();
			AllowedTags = tags.OrderBy(x => x).ToImmutableArray();
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
				&& Formats.Count > 0 && !Formats.Contains(format))
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
}