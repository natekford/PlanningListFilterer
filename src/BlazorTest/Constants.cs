namespace BlazorTest;

public static class IDs
{
	public const string JsonEditor = "json-editor-form";
	public const string SearchModal = "search-modal";

	public static string Query(this string id) => $"#{id}";
}

public static class JSMethods
{
	public const string InitJsonEditor = "initJsonEditor";
}