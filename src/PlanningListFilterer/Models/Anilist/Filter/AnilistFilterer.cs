namespace PlanningListFilterer.Models.Anilist.Filter;

public sealed class AnilistFilterer
{
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
	}

	public async Task UpdateVisibilityAsync()
	{
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
		=> Genres.IsValid(model) && Tags.IsValid(model);
}