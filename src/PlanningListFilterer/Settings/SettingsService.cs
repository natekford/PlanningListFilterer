using Blazored.LocalStorage;

using System.Collections.Concurrent;

namespace PlanningListFilterer.Settings;

public class SettingsService
{
	protected ConcurrentDictionary<Type, object> Cache { get; } = [];
	protected ConcurrentDictionary<Type, string> Keys { get; } = [];
	protected ILocalStorageService LocalStorage { get; }

	public SettingsService(ILocalStorageService localStorage)
	{
		LocalStorage = localStorage;

		RegisterKey<ColumnSettings>("_ColumnSettings");
		RegisterKey<ListSettings>("_ListSettings");
	}

	public async ValueTask<T> GetAsync<T>(
		CancellationToken cancellationToken = default)
		where T : class, new()
	{
		var settings = Cache.GetValueOrDefault(typeof(T)) as T;
		if (settings is not null)
		{
			return settings;
		}

		try
		{
			settings = await LocalStorage.GetItemAsync<T>(
				key: Keys[typeof(T)],
				cancellationToken: cancellationToken
			).ConfigureAwait(false);
		}
		catch
		{
		}

		settings ??= new();
		Cache[typeof(T)] = settings;
		return settings;
	}

	public void RegisterKey<T>(string key)
		=> Keys[typeof(T)] = key;

	public async ValueTask SaveAsync<T>(
		T settings,
		CancellationToken cancellationToken = default)
		where T : class, new()
	{
		await LocalStorage.SetItemAsync(
			key: Keys[typeof(T)],
			data: settings,
			cancellationToken: cancellationToken
		).ConfigureAwait(false);
		Cache[typeof(T)] = settings;
	}
}