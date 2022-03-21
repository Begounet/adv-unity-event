# Changelogs

## 1.3.0

- Supports Property path as custom argument.
- Supports some casts when using dynamic arguments (integers can be used as floats for example).
- Enums as custom arguments are now correctly managed.
- Remove dependency to Odin
- Depends on "type-codebase" and "interface-property-drawer" (optional) packages.
- Improve overall stability
- Improve display of `AUEEvents` while debugging.
- Assert an error if there is an error in AOT generation pipeline
- Add tool `Tools/AdvUnityEvent/Check Validity` to ensure all AUE methods validity in the whole project.
- Add supports of arrays for constant custom arguments.
- Fix UI issues (foldout, method preview names etc.)

## 1.2.0

- UI improvements:

  - `AUEEvent` expands by default if there is any event in it. It improves clarity by directly seeing what events is set when navigating between GameObjects.
  - Generate a method preview in the method selection button. For example, it will now display:<br> `void MyFunc(bool value: {arg0}, string text: "Coucou", int count:  MethodCall(false))`

  - Improve `AUEEvent` display to look more like `UnityEvent` (with event name in header), but keep ability to fold up.

- Add Upgrader class, allowing to transfer data from `UnityEvent` to `AUEEvent`. See documentation for more information.

- Improve internal and UI stability

- Supports static methods. Set the `MonoScript` as a target and select the static method from it.

## 1.1.0

- `AUEGet` now correctly use the return type for method selection
- Return and parameters types now correctly update if the type of the `AUEEvent`/`AUEGet` has changed
- Clean code

## 1.0.0

- Reorderable events
- Can directly use generic `AUEEvent<T>` instead of a creating a new class inheriting from `UnityEvent`
- Method searcher popup (`AddComponent`-like)
- Can use `AUEGet<TResult>` to create a method's result
- Custom parameters:
  - Constant: support all basics native constants + `UnityEngine.Object` + custom serializable class
  - Dynamic: allow to select where you pass your method arguments
  - Method: use nested method as parameter