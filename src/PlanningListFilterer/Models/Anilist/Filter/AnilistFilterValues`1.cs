using System.Collections.Immutable;

namespace PlanningListFilterer.Models.Anilist.Filter;

public abstract class AnilistFilterValues<T> : AnilistFilter
{
	public ImmutableArray<T> Options { get; private set; } = ImmutableArray<T>.Empty;
	public ImmutableHashSet<T> Values { get; private set; } = ImmutableHashSet<T>.Empty;

	protected AnilistFilterValues(AnilistFilterer search) : base(search)
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
		return Parent.UpdateVisibilityAsync();
	}
}