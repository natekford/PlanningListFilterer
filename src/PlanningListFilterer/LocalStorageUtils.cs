using Blazored.LocalStorage;

using System.IO.Compression;
using System.Text.Json;

namespace PlanningListFilterer;

public static class LocalStorageUtils
{
	public static async Task<T> DecodeBase64GZipJsonAsync<T>(
		string base64GZipJson,
		JsonSerializerOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var bytes = Convert.FromBase64String(base64GZipJson);

		await using var msi = new MemoryStream(bytes);
		await using var mso = new MemoryStream();

		await using (var gs = new GZipStream(msi, CompressionMode.Decompress))
		{
			await gs.CopyToAsync(mso, cancellationToken).ConfigureAwait(false);
		}
		mso.Position = 0;

		return (await JsonSerializer.DeserializeAsync<T>(
			utf8Json: mso,
			options: options,
			cancellationToken: cancellationToken
		))!;
	}

	public static async Task<string> EncodeBase64GZipJsonAsync<T>(
		T value,
		JsonSerializerOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		await using var msi = new MemoryStream();
		await using var mso = new MemoryStream();

		await JsonSerializer.SerializeAsync(
			utf8Json: msi,
			value: value,
			options: options,
			cancellationToken: cancellationToken
		).ConfigureAwait(false);
		msi.Position = 0;

		await using (var gs = new GZipStream(mso, CompressionMode.Compress))
		{
			await msi.CopyToAsync(gs, cancellationToken).ConfigureAwait(false);
		}

		return Convert.ToBase64String(mso.ToArray());
	}

	public static async ValueTask<T> GetItemCompressedAsync<T>(
		this ILocalStorageService localStorage,
		string key,
		JsonSerializerOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var encoded = await localStorage.GetItemAsStringAsync(key, cancellationToken).ConfigureAwait(false);
		return await DecodeBase64GZipJsonAsync<T>(
			base64GZipJson: encoded,
			options: options,
			cancellationToken: cancellationToken
		).ConfigureAwait(false);
	}

	public static async ValueTask SetItemCompressedAsync<T>(
		this ILocalStorageService localStorage,
		string key,
		T value,
		JsonSerializerOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var encoded = await EncodeBase64GZipJsonAsync(
			value: value,
			options: options,
			cancellationToken: cancellationToken
		).ConfigureAwait(false);
		await localStorage.SetItemAsStringAsync(
			key: key,
			data: encoded,
			cancellationToken: cancellationToken
		).ConfigureAwait(false);
	}
}