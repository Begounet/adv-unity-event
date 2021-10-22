using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace AUE
{
    [CustomPropertyDrawer(typeof(BaseAUEEvent), useForChildren: true)]
    public class AUEEventPropertyDrawer : PropertyDrawer
    {
        private ReorderableList _reorderableList = null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InitReorderableList(property, label);
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                height += EditorGUIUtility.standardVerticalSpacing;
                height += _reorderableList.GetHeight();
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitReorderableList(property, label);

            Rect lineRect = position;
            lineRect.height = EditorGUIUtility.singleLineHeight;

            property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label);
            if (property.isExpanded)
            {
                position.y = lineRect.yMax;
                position.height -= (lineRect.height + EditorGUIUtility.standardVerticalSpacing);
                _reorderableList.DoList(position);
            }
        }

        private void InitReorderableList(SerializedProperty property, GUIContent label)
        {
            if (_reorderableList != null)
            {
                return;
            }

            var eventsSP = property.FindPropertyRelative("_events");
            _reorderableList = new ReorderableList(property.serializedObject, eventsSP, draggable: true, displayHeader: false, displayAddButton: true, displayRemoveButton: true)
            {
                onAddCallback = (rol) =>
                {
                    eventsSP.InsertArrayElementAtIndex(eventsSP.arraySize);
                    var newItem = eventsSP.GetArrayElementAtIndex(eventsSP.arraySize - 1);
                    newItem.FindPropertyRelative(AUEUtils.TargetSPName).objectReferenceValue = null;
                    newItem.FindPropertyRelative(AUEUtils.MethodNameSPName).stringValue = string.Empty;
                    newItem.FindPropertyRelative(AUEUtils.CallStateSPName).enumValueIndex = (int) UnityEventCallState.RuntimeOnly;
                    SyncArgumentTypes(property, newItem);
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
                }
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