var editor = null;

export async function initJsonEditor(dotNetRef, id, schema, startval) {
	if (editor) {
		editor.destroy();
	}
	const div = document.querySelector(id);

	await new Promise(function (resolve) {
		const temp = createEditor(div, schema, null);
		const listener = async () => {
			temp.off('change', listener);
			await dotNetRef.invokeMethodAsync('OnJsonEditorInstantiated', temp.getValue());
			temp.destroy();
			resolve();
		};
		temp.on('change', listener);
	});

	editor = createEditor(div, schema, startval);
	editor.on('change', async function () {
		const json = editor.getValue();
		const errors = editor.validate();
		await dotNetRef.invokeMethodAsync('OnJsonEditorChanged', json, errors);
	});
};

function createEditor(div, schema, startval) {
	const options = createOptions(schema, startval);
	return new window.JSONEditor(div, options);
}

function createOptions(schema, startval) {
	return {
		iconlib: 'fontawesome5',
		theme: 'bootstrap4',
		show_errors: 'interaction',
		object_layout: 'grid',
		disable_edit_json: true,
		disable_properties: true,
		show_opt_in: true,
		schema: schema,
		startval: startval,
	};
}