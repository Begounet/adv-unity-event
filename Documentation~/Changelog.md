# Changelogs

## 1.3.2

- Add:
  - Can subscribe to `AUEEvent` at runtime.
  - Unit Tests for pretty name generation + `AUEEvent` runtime event subscription
  - In editor, display the scene in which an error in `AUESimpleMethod` could occur.
- Fix:
  - Force binding flags initialization on `AUEEvent` because Unity does not initialize serialized array's items.

## 1.3.1

- Add:
  - Log more details when an error occurs with an invalid method (can even provide the scene path when in editor)
  - Provides a way to defines how to react to an exception being raised from a Property custom argument (ie. if null occurs in the middle of path to the property).
  - Add the static `BoolSH.Invert` method to easily invert a boolean value.
  - `AUEFrontUtils` to manage `AUEEvent` and `AUEMethod` from external editor scripts easily and in a safe way.
- Fixes:
  - Invalid cast from float to integer
  - Correctly use Binding Flags to get the AUE methods
  - Static binding flags was not set by default in `AUESimpleMethod`
  - Reset correctly cast settings when targeted method's parameters have changed
  - Fix method parameters wrongly registered for AOT
  - Update correctly the method name if the argument types change
  - Stop checking target validity when registering for AOT (because it is currently not possible from the serialization system)
  - Fix `AUEMethod` issues when being in the same array of `AUEEvent`. Modifying one was modifying the others.
- Refacto:
  - `SerializableType` now use the `TypeSelectorGUI` to select a type.
  - Code cleaning and minor stuff
  - Improve overall stability (with a better error management and other fixes)
  - Use static `BindingFlags` for default value instead of using the same combos of `BindingFlags` at multiple place.

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