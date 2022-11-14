using BlazorTest.Models;

using System.Collections.Immutable;
using System.Net.Http.Json;

namespace BlazorTest.Pages;

public partial class AnilistPlanning
{
	public List<Media> Entries { get; set; } = new();
	public MediaSearch Search { get; set; } = new(Enumerable.Empty<Media>());
	public string? Username { get; set; }

	private async Task LoadEntries()
	{
#if false
		var response = await AnilistResponse.GetAnilistResponseAsync(Http, username!).ConfigureAwait(false);
#else
		var response = (await Http.GetFromJsonAsync<AnilistResponse>(
			requestUri: "sample-data/anilistresponse5.json",
			options: AnilistResponse.JsonOptions
		).ConfigureAwait(false))!;
#endif

		Entries = response.Data.MediaListCollection.Lists
			.SelectMany(l => l.Entries.Select(e => e.Media))
			.Where(x => x?.Status == MediaStatus.FINISHED)
			.OrderBy(x => x.Id)
			.ToList();

		Search = await MediaSearch.CreateAsync(Entries).ConfigureAwait(false);
	}

	public sealed class MediaSearch
	{
		private readonly IEnumerable<Media> _Media;

		public ImmutableArray<MediaFormat> AllowedFormats { get; private set; } = ImmutableArray<MediaFormat>.Empty;
		public ImmutableArray<string> AllowedGenres { get; private set; } = ImmutableArray<string>.Empty;
		public ImmutableArray<string> AllowedTags { get; private set; } = ImmutableArray<string>.Empty;
		public ImmutableHashSet<MediaFormat> Formats { get; set; } = ImmutableHashSet<MediaFormat>.Empty;
		public ImmutableHashSet<string> Genres { get; set; } = ImmutableHashSet<string>.Empty;
		public bool IsModalActive { get; private set; }
		public int? MaximumDuration { get; private set; }
		public int? MaximumYear { get; private set; }
		public int? MinimumDuration { get; private set; }
		public int? MinimumYear { get; private set; }
		public ImmutableHashSet<string> Tags { get; set; } = ImmutableHashSet<string>.Empty;

		public MediaSearch(IEnumerable<Media> media)
		{
			_Media = media;
		}

		public static async Task<MediaSearch> CreateAsync(IEnumerable<Media> media)
		{
			var vm = new MediaSearch(media);
			await vm.UpdateVisibilityAsync().ConfigureAwait(false);
			return vm;
		}

		public Task Clear()
		{
			Formats = ImmutableHashSet<MediaFormat>.Empty;
			Genres = ImmutableHashSet<string>.Empty;
			Tags = ImmutableHashSet<string>.Empty;
			MaximumDuration = null;
			MinimumDuration = null;
			MaximumYear = null;
			MinimumYear = null;
			return UpdateVisibilityAsync();
		}

		public Task SetFormats(IEnumerable<MediaFormat> formats)
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
			var formats = new HashSet<MediaFormat>();
			var genres = new HashSet<string>();
			var tags = new HashSet<string>();

			foreach (var media in _Media)
			{
				// visbility doesn't matter for formats
				if (media.Format is MediaFormat format)
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

		private bool GetUpdatedVisibility(Media media)
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
			if (media.Format is MediaFormat format
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