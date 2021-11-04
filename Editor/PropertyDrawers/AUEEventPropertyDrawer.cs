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

        private ReorderableList _reorderableList = null;
        private GUIContent _label;
        private bool _isStateInitialized = false;

        private static readonly MethodInfo DoListHeaderMI = typeof(ReorderableList).GetMethod("DoListHeader", BindingFlags.NonPublic | BindingFlags.Instance);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitReorderableList(property);
            InitializeState(property);
            float height = property.isExpanded ? _reorderableList.GetHeight() : EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheLabel(property, label);
            InitReorderableList(property);
            InitializeState(property);
            if (property.isExpanded)
            {
                _reorderableList.DoList(position);
            }
            else
            {
                DrawOnlyHeader(position);
            }
        }

        private void InitializeState(SerializedProperty property)
        {
            if (_isStateInitialized)
            {
                return;
            }

            property.isExpanded = (property.FindPropertyRelative(EventsSPName).arraySize > 0);
            _isStateInitialized = true;
        }

        private void CacheLabel(SerializedProperty property, GUIContent baseLabel)
        {
            if (_label != null)
            {
                return;
            }

            var argumentTypesSP = property.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);
            var argumentTypesSB = new StringBuilder();
            for (int i = 0; i < argumentTypesSP.arraySize; ++i)
            {
                Type argumentType = SerializableTypeHelper.LoadType(argumentTypesSP.GetArrayElementAtIndex(i));
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
            _label = new GUIContent($"{baseLabel.text}({argumentTypesSB.ToString()})", baseLabel.tooltip);
        }

        private void DrawOnlyHeader(Rect position)
        {
            DoListHeaderMI.Invoke(_reorderableList, new object[] { position });
        }

        private void InitReorderableList(SerializedProperty property)
        {
            if (_reorderableList != null)
            {
                return;
            }

            var eventsSP = property.FindPropertyRelative(EventsSPName);
            _reorderableList = new ReorderableList(property.serializedObject, eventsSP, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true)
            {
                onAddCallback = (rol) =>
                {
                    eventsSP.InsertArrayElementAtIndex(eventsSP.arraySize);
                    var newItem = eventsSP.GetArrayElementAtIndex(eventsSP.arraySize - 1);
                    newItem.FindPropertyRelative(AUEUtils.TargetSPName).objectReferenceValue = null;
                    newItem.FindPropertyRelative(AUEUtils.MethodNameSPName).stringValue = string.Empty;
                    newItem.FindPropertyRelative(AUEUtils.CallStateSPName).enumValueIndex = (int)UnityEventCallState.RuntimeOnly;
                    SyncArgumentTypes(property, newItem);
                },
                drawHeaderCallback = (Rect headerRect) =>
                {
                    headerRect.xMin += 10;
                    headerRect.height = 18f;
                    property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, _label);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var eventSP = eventsSP.GetArrayElementAtIndex(index);
                    SyncArgumentTypes(property, eventSP);
                    EditorGUI.PropertyField(rect, eventSP, eventsSP.isExpanded);
                },
                elementHeightCallback = (index) =>
                {
                    var eventSP = eventsSP.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(eventSP, eventsSP.isExpanded);
                },
            };
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
    }
}