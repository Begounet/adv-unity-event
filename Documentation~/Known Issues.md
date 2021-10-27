# Known Issues

## `Unknown managed type referenced: [<old asm def name>] AUE.<AUECADynamic|AUECAConstant|AUECAMethod>  `

This message appears if the AdvUnityEvent asmdef has changed its name. Because `[SerializeReference]` is based on the assembly name, it will not be able to load the correct class. Theses classes correspond to the mode of a parameter.

You can fix the issue by opening the asset file (scene/prefab/etc.) in a text editor and replace the old asm def by the new one. The line should look like this:

```csharp
references:
    version: 1
    00000000:
		type: {class: AUECADynamic, ns: AUE, asm: OldAsmDefName}
```

## Unsupported copy/paste

Copy/paste is not supported for now and its behavior is unknown. And further, you should not use it since the result of the paste can corrupt the internal behavior of AUEEvent.

