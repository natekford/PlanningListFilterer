using System.Collections.Immutable;

namespace PlanningListFilterer.Models.Anilist.Filter;

public abstract class AnilistFilterValues<T>
{
	public ImmutableArray<T> Options { get; private set; } = ImmutableArray<T>.Empty;
	public ImmutableHashSet<T> Values { get; private set; } = ImmutableHashSet<T>.Empty;
	protected AnilistFilterer Parent { get; }

	protected AnilistFilterValues(AnilistFilterer parent)
	{
		Parent = parent;
	}

	public abstract bool IsValid(AnilistModel model);

	public void Reset()
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