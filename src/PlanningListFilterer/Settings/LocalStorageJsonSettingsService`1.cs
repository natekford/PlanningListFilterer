using Blazored.LocalStorage;

namespace PlanningListFilterer.Settings;

public abstract class LocalStorageJsonSettingsService<T> where T : new()
{
	protected abstract string Key { get; }
	protected ILocalStorageService LocalStorage { get; }
	protected T? Settings { get; set; }

	protected LocalStorageJsonSettingsService(ILocalStorageService localStorage)
	{
		LocalStorage = localStorage;
	}

	public async ValueTask<T> GetSettingsAsync(
		CancellationToken cancellationToken = default)
	{
		if (Settings is not null)
		{
			return Settings;
		}

		try
		{
			Settings = await LocalStorage.GetItemAsync<T>(
				key: Key,
				cancellationToken: cancellationToken
			).ConfigureAwait(false);
		}
		catch
		{
		}

		return Settings ??= new();
	}

	public async ValueTask SaveSettingsAsync(
		T settings,
		CancellationToken cancellationToken = default)
	{
		await LocalStorage.SetItemAsync(
			key: Key,
			data: settings,
			cancellationToken: cancellationToken
		).ConfigureAwait(false);
		Settings = settings;
	}
}