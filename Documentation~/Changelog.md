# Changelogs

## 1.0.0

- Reorderable events
- Can directly use generic `AUEEvent<T>` instead of a creating a new class inheriting from `UnityEvent`
- Method searcher popup (`AddComponent`-like)
- Can use `AUEGet<TResult>` to create a method's result
- Custom parameters:
  - Constant: support all basics native constants + UnityEngine.Object + custom serializable class
  - Dynamic: allow to select where you pass your method arguments
  - Method: use nested method as parameter