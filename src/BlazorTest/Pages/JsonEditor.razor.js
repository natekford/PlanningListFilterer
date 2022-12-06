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

			const fakerDefault = await createFakerDefault(schema);
			const editorDefault = temp.getValue();
			const combined = { ...fakerDefault, ...editorDefault };
			console.log('Default JSON:');
			console.log(combined);

			await dotNetRef.invokeMethodAsync('OnJsonEditorInstantiated', combined);
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
	const options = {
		disable_edit_json: true,
		disable_properties: true,
		iconlib: 'fontawesome5',
		object_layout: 'grid',
		schema: schema,
		show_errors: 'interaction',
		show_opt_in: true,
		startval: startval,
		theme: 'bootstrap4',
	};
	return new window.JSONEditor(div, options);
}

async function createFakerDefault(schema) {
	const options = {
		alwaysFakeOptionals: true,
		fillProperties: false,
		random: () => 0,
		replaceEmptyByRandomValue: false,
		sortProperties: true,
		useDefaultValue: true,
	};
	window.JSONSchemaFaker.option(options);
	return await window.JSONSchemaFaker.resolve(schema);
}