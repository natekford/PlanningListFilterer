using Json.More;
using Json.Schema;

using Microsoft.JSInterop;

using MudBlazor.Extensions;

using System.Collections;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlazorTest.Pages;

public sealed partial class JsonEditor : IDisposable
{
	private readonly DotNetObjectReference<JsonEditor> _DotNetReference;
	private readonly JsonSerializerOptions _Options = new()
	{
		WriteIndented = true,
	};
	private JsonNode? _Default;
	public string? Errors { get; set; }
	public string? Json { get; set; }

	public JsonEditor()
	{
		_DotNetReference = DotNetObjectReference.Create(this);
	}

	public static bool RemoveDefaults(JsonNode? @default, JsonNode? node)
	{
		// node == defaults even if both are null
		if (@default is null)
		{
			return node is null;
		}
		// default has a value, node is null, user cannot provide a null value
		// means to remove this value
		else if (node is null)
		{
			return true;
		}
		else if (@default.GetType() != node.GetType())
		{
			throw new InvalidOperationException(
				$"Defaults and current JSON node are not the same type at '{node.GetPath()}'.");
		}

		// loop through all array values
		if (@default is JsonArray jDefaultArray && node is JsonArray jArray)
		{
			for (var i = jDefaultArray.Count - 1; i >= 0; --i)
			{
				if (RemoveDefaults(jDefaultArray[i], jArray[i]))
				{
					jArray.RemoveAt(i);
				}
			}
			return RemoveEmpty(jArray);
		}
		// loop through all properties
		else if (@default is JsonObject jDefaultObj && node is JsonObject jObj)
		{
			foreach (var (propertyName, jDefaultProperty) in jDefaultObj)
			{
				if (RemoveDefaults(jDefaultProperty, jObj[propertyName]))
				{
					jObj.Remove(propertyName);
				}
			}
			return RemoveEmpty(jObj);
		}
		// check if json strings are the same
		else if (@default is JsonValue jDefaultValue && node is JsonValue jValue)
		{
			var jDefault = jDefaultValue.AsJsonString();
			var jString = jValue.AsJsonString();
			Console.WriteLine(
				$"{jValue.GetPath()}\n" +
				$"Default: {jDefault}\n" +
				$"Current: {jString}"
			);
			return jDefault == jString || RemoveEmpty(jValue);
		}
		else
		{
			throw new ArgumentException("Not a valid JSON type.", nameof(node));
		}
	}

	public static bool RemoveEmpty(JsonNode? node)
	{
		if (node is JsonArray jArray)
		{
			for (var i = jArray.Count - 1; i >= 0; --i)
			{
				if (RemoveEmpty(jArray[i]))
				{
					jArray.RemoveAt(i);
				}
			}
			return jArray.Count == 0;
		}
		else if (node is JsonObject jObj)
		{
			foreach (var (propertyName, jProperty) in jObj.ToList())
			{
				if (RemoveEmpty(jProperty))
				{
					jObj.Remove(propertyName);
				}
			}
			return jObj.Count == 0;
		}
		else if (node is JsonValue jValue)
		{
			var jStr = jValue.ToJsonString();
			// empty strings still get quotation marks around them
			if (jStr is string s && s.StartsWith('"') && s.EndsWith('"'))
			{
				jStr = s[1..^1];
			}
			return string.IsNullOrWhiteSpace(jStr);
		}
		else
		{
			throw new ArgumentException("Not a valid JSON type.", nameof(node));
		}
	}

	public void Dispose()
		=> _DotNetReference.Dispose();

	[JSInvokable]
	public Task OnJsonEditorChanged(JsonElement obj, JsonErrors[] errors)
	{
		Errors = JsonSerializer.Serialize(errors, _Options);

		if (errors.Length == 0)
		{
			var node = obj.AsNode()!;
			RemoveDefaults(_Default, node);
			Json = JsonSerializer.Serialize(node, _Options);
		}
		else
		{
			Json = "Invalid";
		}

		StateHasChanged();
		return Task.CompletedTask;
	}

	[JSInvokable]
	public Task OnJsonEditorInstantiated(JsonElement obj)
	{
		_Default = obj.AsNode();
		Json = "{}";

		StateHasChanged();
		return Task.CompletedTask;
	}

	protected override async Task OnInitializedAsync()
	{
		var schema = (await Http.GetFromJsonAsync<JsonSchema>(
			requestUri: "sample-data/sampleschema.json?a=3"
		).ConfigureAwait(false))!;

		await using var module = await JS.InvokeAsync<IJSObjectReference>(
			identifier: "import",
			args: "./Pages/JsonEditor.razor.js"
		).ConfigureAwait(false);

		await module.InvokeVoidAsync(
			identifier: JSMethods.InitJsonEditor,
			args: new object[]
			{
				_DotNetReference,
				IDs.JsonEditor.Query(),
				schema,
				new
				{
					location = new
					{
						city = "Petaluma",
					},
				},
			}
		).ConfigureAwait(false);
	}

	public record JsonErrors(
		string Path,
		string Property,
		string Message,
		int ErrorCount
	);
}