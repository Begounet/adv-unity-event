# Upgrader

This document aims to explain how to use the Upgrader, to update from `UnityEvent` to `AUEEvent` without destroying your whole project.

## What's for?

If you want to replace an `UnityEvent` by an `AUEEvent` in a class, all your bound data from the `UnityEvent` will be lost if you just change the type.

By using the Upgrader, you can transfer the data from the `UnityEvent` to the `AUEEvent`, then get rid of the `UnityEvent`.

![Example of data transferred automatically](Resources/Upgrader.jpg)

## Setup

There is 2 steps:

- transfer data from `UnityEvent` to `AUEEvent`

  - Make your class inheriting from `ISerializationCallbackReceiver`

  - In `OnBeforeSerialize`, and add the line `AUE.Upgrader.ToAUEEvent(_unityEvent, _aueEvent);`

  - Recompile

  - From the Unity menu bar, start `Tools > AdvUnityEvent > Upgrade`. It will stall the editor during the upgrading.

    *Note: The upgrading consist of just forcing reserialization of all scenes and prefabs in the project so you are sure everything is upgraded.

- delete the `UnityEvent` and remove `ISerializationCallbackReceiver` methods and interface if you don't need them anymore