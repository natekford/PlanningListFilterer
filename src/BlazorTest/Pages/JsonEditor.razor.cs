using Json.More;
using Json.Schema;

using Microsoft.JSInterop;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BlazorTest.Pages;

public partial class JsonEditor
{
	private readonly JsonSerializerOptions _Options = new()
	{
		WriteIndented = true,
	};
	private readonly DotNetObjectReference<JsonEditor> _Ref;

	private JsonNode? _Defaults;
	public string? Errors { get; set; }
	public string? Json { get; set; }

	public JsonEditor()
	{
		_Ref = DotNetObjectReference.Create(this);
	}

	public static bool RemoveDefaults(JsonNode? defaults, JsonNode? node)
	{
		// node == defaults even if both ar enull
		if (defaults is null)
		{
			return node is null;
		}
		// default has a value, node is null, keep null value in serialization
		else if (node is null)
		{
			return false;
		}
		else if (defaults.GetType() != node.GetType())
		{
			throw new InvalidOperationException(
				$"Defaults and current JSON node are not the same type at '{node.GetPath()}'.");
		}

		var isDefaultValue = true;
		// loop through all array values
		if (node is JsonArray jArray)
		{
			var jDefaultArray = (JsonArray)defaults;
			isDefaultValue = jArray.Count == jDefaultArray.Count;
			for (var i = jDefaultArray.Count - 1; i >= 0; --i)
			{
				if (RemoveDefaults(jDefaultArray[i], jArray[i]))
				{
					jArray.RemoveAt(i);
				}
				else
				{
					isDefaultValue = false;
				}
			}
		}
		// loop through all properties
		else if (node is JsonObject jObj)
		{
			var jDefault = (JsonObject)defaults;
			foreach (var (propertyName, jDefaultProperty) in jDefault)
			{
				if (RemoveDefaults(jDefaultProperty, jObj[propertyName]))
				{
					jObj.Remove(propertyName);
				}
				else
				{
					isDefaultValue = false;
				}
			}
		}
		// check if json strings are the same
		else if (node is JsonValue jValue)
		{
			var jDefault = defaults.AsJsonString();
			var jString = jValue.AsJsonString();
			isDefaultValue = jDefault == jString;
		}
		else
		{
			throw new ArgumentException("Not a valid JSON type.", nameof(node));
		}

		return isDefaultValue;
	}

	[JSInvokable]
	public Task OnJsonEditorChanged(JsonElement obj, JsonErrors[] errors)
	{
		Errors = JsonSerializer.Serialize(errors, _Options);

		if (errors.Length == 0)
		{
			var node = obj.AsNode()!;
			// If default node is null, the first change event indicates that the
			// js library has created an object from the schema
			if (_Defaults is null)
			{
				_Defaults = node;
				Json = "{}";
			}
			// If default node is not null, the user has changed a value in the ui
			else
			{
				RemoveDefaults(_Defaults, node);
				Json = JsonSerializer.Serialize(node, _Options);
			}
		}
		else
		{
			Json = "Invalid";
		}

		StateHasChanged();
		return Task.CompletedTask;
	}

	protected override async Task OnInitializedAsync()
	{
		var schema = (await Http.GetFromJsonAsync<JsonDocument>(
			requestUri: "sample-data/sampleschema.json"
		).ConfigureAwait(false))!;

		var module = await JS.InvokeAsync<IJSObjectReference>(
			identifier: "import",
			args: "./Pages/JsonEditor.razor.js"
		).ConfigureAwait(false);

		await module.InvokeVoidAsync(
			identifier: JSMethods.InitJsonEditor,
			args: new object[]
			{
				IDs.JsonEditor.Query(),
				schema,
				_Ref
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