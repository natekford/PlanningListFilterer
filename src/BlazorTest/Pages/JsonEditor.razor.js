var jsonEditor = null;

export async function initJsonEditor(id, schema, dotNetRef) {
	const options = {
		iconlib: 'https://use.fontawesome.com/releases/v5.6.1/css/all.css',
		theme: 'bootstrap4',
		show_errors: 'interaction',
		object_layout: 'grid',
		disable_edit_json: true,
		disable_properties: true,
		show_opt_in: false,
		schema: schema,
	};

	if (jsonEditor) {
		jsonEditor.destroy();
	}

	const jsonEditorForm = document.querySelector(id);
	jsonEditor = new window.JSONEditor(jsonEditorForm, options);

	jsonEditor.on('change', function () {
		const json = jsonEditor.getValue();
		const errors = jsonEditor.validate();
		dotNetRef.invokeMethodAsync('OnJsonEditorChanged', json, errors);
	});
};