var editor = null;

export async function initJsonEditor(id, schema, dotNetRef) {
	if (editor) {
		editor.destroy();
	}

	const options = {
		iconlib: 'https://use.fontawesome.com/releases/v5.6.1/css/all.css',
		theme: 'bootstrap4',
		show_errors: 'interaction',
		object_layout: 'grid',
		disable_edit_json: true,
		disable_properties: true,
		show_opt_in: true,
		schema: schema,
	};
	const jsonEditorForm = document.querySelector(id);

	editor = new window.JSONEditor(jsonEditorForm, options);
	editor.on('change', function () {
		const json = editor.getValue();
		const errors = editor.validate();
		dotNetRef.invokeMethodAsync('OnJsonEditorChanged', json, errors);
	});
};