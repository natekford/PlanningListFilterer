using System.Collections.Immutable;

namespace PlanningListFilterer.Models.Anilist.Search;

public abstract class AnilistSearchValues<T> : AnilistSearchItem
{
	public ImmutableArray<T> Options { get; private set; } = ImmutableArray<T>.Empty;
	public ImmutableHashSet<T> Values { get; private set; } = ImmutableHashSet<T>.Empty;

	protected AnilistSearchValues(AnilistSearch search) : base(search)
	{
	}

	public override void Reset()
	{
		Options = ImmutableArray<T>.Empty;
		Values = ImmutableHashSet<T>.Empty;
	}

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
		return Search.UpdateVisibilityAsync();
	}
}