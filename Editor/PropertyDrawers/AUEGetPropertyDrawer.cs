using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(BaseAUEGet), useForChildren: true)]
    public class AUEGetPropertyDrawer : PropertyDrawer
    {
        private const string MethodSPName = "_method";
        private const string ReturnTypeSPName = "_returnType";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                height += EditorGUIUtility.standardVerticalSpacing;

                var methodSP = property.FindPropertyRelative(MethodSPName);
                height += EditorGUI.GetPropertyHeight(methodSP, label, methodSP.isExpanded);
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect lineRect = position;
            lineRect.height = EditorGUIUtility.singleLineHeight;

            var methodSP = property.FindPropertyRelative(MethodSPName);

            property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label);
            lineRect.y += lineRect.height + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                Rect propRect = new Rect(position.x, lineRect.y, position.width, position.height - lineRect.yMax);
                EditorGUI.PropertyField(propRect, methodSP, label, methodSP.isExpanded);
            }
        }
    }
}