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
			return $"{value.Month}/{value.Year}";
		});
		Map(x => x.Format);
		Map(x => x.Episodes);
		Map(x => x.Duration);
		Map(x => x.IsSequel);
		Map(x => x.AverageScore);
		Map(x => x.Popularity);
		Map(x => x.FriendScore);
		Map(x => x.FriendPopularityScored);
		Map(x => x.FriendPopularityTotal);
		Map(x => x.PersonalScore);
		Map(x => x.ScoreDiffAverage);
		Map(x => x.ScoreDiffFriends);
		Map(x => x.Genres).Convert(args =>
		{
			var value = args.Value.Genres.Order();
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