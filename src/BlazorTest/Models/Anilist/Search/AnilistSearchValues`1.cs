using System.Collections.Immutable;

namespace BlazorTest.Models.Anilist.Search;

public abstract class AnilistSearchValues<T> : IAnilistSearchItem
{
	private readonly AnilistSearch _Search;

	public ImmutableArray<T> Options { get; private set; } = ImmutableArray<T>.Empty;
	public ImmutableHashSet<T> Values { get; private set; } = ImmutableHashSet<T>.Empty;

	protected AnilistSearchValues(AnilistSearch search)
	{
		_Search = search;
	}

	public abstract bool IsValid(AnilistModel model);

	public void SetOptions(IEnumerable<T> options)
	{
		Options = options
			.OrderByDescending(Values.Contains)
			.ThenBy(x => x)
			.ToImmutableArray();
	}

	public Task SetValues(IEnumerable<T> values)
	{
		Values = values.ToImmutableHashSet();
		return _Search.UpdateVisibilityAsync();
	}

	void IAnilistSearchItem.Clear()
	{
		Options = ImmutableArray<T>.Empty;
		Values = ImmutableHashSet<T>.Empty;
	}
}