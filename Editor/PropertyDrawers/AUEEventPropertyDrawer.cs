using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace AUE
{
    [CustomPropertyDrawer(typeof(BaseAUEEvent), useForChildren: true)]
    public class AUEEventPropertyDrawer : PropertyDrawer
    {
        private const string EventsSPName = "_events";

        private class State
        {
            public ReorderableList ReorderableList = null;
            public GUIContent Label;
            public bool IsStateInitialized = false;
            public Type[] CachedArgumentTypes;
        }

        private Dictionary<string, State> _states = new Dictionary<string, State>();

        private static readonly MethodInfo DoListHeaderMI = typeof(ReorderableList).GetMethod("DoListHeader", BindingFlags.NonPublic | BindingFlags.Instance);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            State state = CreateOrGetCachedState(property);
            InitReorderableList(property, state);
            InitializeState(property, state);
            float height = property.isExpanded ? state.ReorderableList.GetHeight() : EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            State state = CreateOrGetCachedState(property);
            CacheLabel(property, label, state);
            InitReorderableList(property, state);
            InitializeState(property, state);
            if (property.isExpanded)
            {
                state.ReorderableList.DoList(position);
            }
            else
            {
                DrawOnlyHeader(position, state.ReorderableList);
            }
        }

        private void InitializeState(SerializedProperty property, State state)
        {
            if (state.IsStateInitialized)
            {
                return;
            }

            property.isExpanded = (property.FindPropertyRelative(EventsSPName).arraySize > 0);
            state.IsStateInitialized = true;
        }

        private void CacheLabel(SerializedProperty property, GUIContent baseLabel, State state)
        {
            if (state.Label != null && !HasArgumentsChanged(property, state))
            {
                return;
            }

            var argumentTypesSP = property.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);
            var argumentTypesSB = new StringBuilder();
            state.CachedArgumentTypes = new Type[argumentTypesSP.arraySize];
            for (int i = 0; i < argumentTypesSP.arraySize; ++i)
            {
                Type argumentType = SerializableTypeHelper.LoadType(argumentTypesSP.GetArrayElementAtIndex(i));
                state.CachedArgumentTypes[i] = argumentType;
                if (argumentType != null)
                {
                    argumentTypesSB.Append(AUEUtils.MakeHumanDisplayType(argumentType));
                }
                else
                {
                    argumentTypesSB.Append("<undefined>");
                }
                if (i + 1 < argumentTypesSP.arraySize)
                {
                    argumentTypesSB.Append(", ");
                }
            }
            state.Label = new GUIContent($"{baseLabel.text}({argumentTypesSB.ToString()})", baseLabel.tooltip);
        }

        private bool HasArgumentsChanged(SerializedProperty property, State state)
        {
            Type[] cachedArgumentTypes = state.CachedArgumentTypes;
            if (cachedArgumentTypes == null)
            {
                return true;
            }

            var argumentTypesSP = property.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);
            if (cachedArgumentTypes.Length != argumentTypesSP.arraySize)
            {
                return true;
            }

            for (int i = 0; i < argumentTypesSP.arraySize; ++i)
            {
                Type argumentType = SerializableTypeHelper.LoadType(argumentTypesSP.GetArrayElementAtIndex(i));
                if (cachedArgumentTypes[i] != argumentType)
                {
                    return true;
                }
            }

            return false;
        }

        private void DrawOnlyHeader(Rect position, ReorderableList reorderableList)
        {
            DoListHeaderMI.Invoke(reorderableList, new object[] { position });
        }

        private void InitReorderableList(SerializedProperty property, State state)
        {
            if (state.ReorderableList != null)
            {
                return;
            }

            var eventsSP = property.FindPropertyRelative(EventsSPName);
            state.ReorderableList = new ReorderableList(property.serializedObject, eventsSP, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true)
            {
                onAddCallback = (rol) =>
                {
                    eventsSP.InsertArrayElementAtIndex(eventsSP.arraySize);
                    var newEventSP = eventsSP.GetArrayElementAtIndex(eventsSP.arraySize - 1);
                    InitializeNewAUEEvent(property, newEventSP);
                },
                drawHeaderCallback = (Rect headerRect) =>
                {
                    int indentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0;
                    {
                        headerRect.xMin += 10;
                        headerRect.height = 18f;
                        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, state.Label);
                    }
                    EditorGUI.indentLevel = indentLevel;
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    try
                    {
                        if (index >= 0 && index < eventsSP.arraySize)
                        {
                            var eventSP = eventsSP.GetArrayElementAtIndex(index);
                            SyncArgumentTypes(property, eventSP);
                            EditorGUI.PropertyField(rect, eventSP, eventsSP.isExpanded);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                },
                elementHeightCallback = (index) =>
                {
                    if (index >= 0 && index < eventsSP.arraySize)
                    {
                        var eventSP = eventsSP.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(eventSP, eventsSP.isExpanded);
                    }
                    return 0.0f;
                },
            };
        }

        private void InitializeNewAUEEvent(SerializedProperty property, SerializedProperty aueEventSP)
        {
            aueEventSP.FindPropertyRelative(AUEUtils.TargetSPName).objectReferenceValue = null;
            aueEventSP.FindPropertyRelative(AUEUtils.MethodNameSPName).stringValue = string.Empty;
            aueEventSP.FindPropertyRelative(AUEUtils.CallStateSPName).enumValueIndex = (int)UnityEventCallState.RuntimeOnly;

            aueEventSP.FindPropertyRelative(AUEUtils.BindingFlagsSPName).intValue = (int)DefaultBindingFlags.AUESimpleMethod;

            SyncArgumentTypes(property, aueEventSP);
        }

        private void SyncArgumentTypes(SerializedProperty aueSP, SerializedProperty aueMethodSP)
        {
            var aueArgumentTypesSP = aueSP.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);
            var aueMethodArgumentTypesSP = aueMethodSP.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);
            aueMethodArgumentTypesSP.arraySize = aueArgumentTypesSP.arraySize;
            for (int i = 0; i < aueArgumentTypesSP.arraySize; ++i)
            {
                var aueArgumentTypeSP = aueArgumentTypesSP.GetArrayElementAtIndex(i);
                var aueMethodArgumentTypeSP = aueMethodArgumentTypesSP.GetArrayElementAtIndex(i);
                SerializableTypeHelper.CopySerializableType(aueArgumentTypeSP, aueMethodArgumentTypeSP);
            }
        }

        private State CreateOrGetCachedState(SerializedProperty sp)
        {
            if (!_states.TryGetValue(sp.propertyPath, out State state))
            {
                state = new State();
                _states.Add(sp.propertyPath, state);
            }
            return state;
        }
    }
}