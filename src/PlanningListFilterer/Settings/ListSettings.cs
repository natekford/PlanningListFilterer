using Blazored.LocalStorage;

namespace PlanningListFilterer.Settings;

public sealed record ListSettings(
	bool EnableFriendScores = false
);

public class ListSettingsService
{
	private const string KEY = "ListSettings";
	private readonly ILocalStorageService _LocalStorage;
	private ListSettings? _Settings;

	public ListSettingsService(ILocalStorageService localStorage)
	{
		_LocalStorage = localStorage;
	}

	public async ValueTask<ListSettings> GetSettingsAsync(
		CancellationToken cancellationToken = default)
	{
		if (_Settings is not null)
		{
			return _Settings;
		}

		try
		{
			_Settings = await _LocalStorage.GetItemAsync<ListSettings>(
				key: KEY,
				cancellationToken: cancellationToken
			).ConfigureAwait(false);
		}
		catch
		{
		}

		return _Settings ??= new();
	}

	public async ValueTask SaveSettingsAsync(
		ListSettings settings,
		CancellationToken cancellationToken = default)
	{
		await _LocalStorage.SetItemAsync(
			key: KEY,
			data: settings,
			cancellationToken: cancellationToken
		).ConfigureAwait(false);
		_Settings = settings;
	}
}