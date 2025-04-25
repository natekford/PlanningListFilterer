using CsvHelper.Configuration;

namespace PlanningListFilterer.Models.Anilist;

public class AnilistModelMap : ClassMap<AnilistModel>
{
	public AnilistModelMap()
	{
		Map(x => x.Id);
		Map(x => x.Title);
		Map(x => x.Start).Convert(args =>
		{
			var value = args.Value.Start;
			return DateOnly.FromDateTime(value).ToString();
		});
		Map(x => x.Format);
		Map(x => x.Episodes);
		Map(x => x.Duration);
		Map(x => x.IsSequel);
		Map(x => x.AverageScore);
		Map(x => x.Popularity);
		Map(x => x.FriendScore);
		Map(x => x.FriendPopularityTotal);
		Map(x => x.FriendPopularityScored);
		Map(x => x.Genres).Convert(args =>
		{
			var value = args.Value.Genres.OrderBy(x => x);
			return string.Join("\r\n", value);
		});
		Map(x => x.Tags).Convert(args =>
		{
			var value = args.Value.Tags
				.OrderByDescending(x => x.Value)
				.Select(x => $"{x.Key} ({x.Value}%)");
			return string.Join("\r\n", value);
		});
	}
}