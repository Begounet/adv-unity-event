using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static AUE.AUEMethodParameterInfo;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUEMethodParameterInfo))]
    public class AUEMethodParameterInfoPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var customMethodSP = property.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
                height += EditorGUI.GetPropertyHeight(customMethodSP, customMethodSP.isExpanded);
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect lineRect = position;
            lineRect.height = EditorGUIUtility.singleLineHeight;

            property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label);
            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                ++EditorGUI.indentLevel;

                var customArgumentSP = property.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
                var modeSP = property.FindPropertyRelative(AUEUtils.ModeSPName);
                InitializeCustomArgumentIFN(customArgumentSP, (EMode)modeSP.enumValueIndex);

                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.PropertyField(lineRect, modeSP);
                    lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateCustomArgumentType(property, customArgumentSP, (EMode)modeSP.enumValueIndex);
                }

                Rect propRect = new Rect(lineRect.x, lineRect.y, lineRect.width, EditorGUI.GetPropertyHeight(customArgumentSP, includeChildren: customArgumentSP.isExpanded));
                EditorGUI.PropertyField(propRect, customArgumentSP, includeChildren: customArgumentSP.isExpanded);

                --EditorGUI.indentLevel;
            }
        }

        private void UpdateCustomArgumentType(SerializedProperty parameterInfoSP, SerializedProperty customArgumentSP, EMode mode)
        {
            TryDeleteMethodFromDatabase(parameterInfoSP, customArgumentSP);
            InitializeCustomArgument(customArgumentSP, mode);
        }

        private void InitializeCustomArgument(SerializedProperty customArgumentSP, EMode mode)
        {
            object refValue = null;
            switch (mode)
            {
                case EMode.Dynamic:
                    refValue = new AUECADynamic();
                    break;
                case EMode.Constant:
                    refValue = new AUECAConstant();
                    break;
                case EMode.Method:
                    refValue = new AUECAMethodReference();
                    break;
            }
            customArgumentSP.managedReferenceValue = refValue;
        }

        private void InitializeCustomArgumentIFN(SerializedProperty customArgumentSP, EMode mode)
        {
            if (string.IsNullOrEmpty(customArgumentSP.managedReferenceFullTypename))
            {
                InitializeCustomArgument(customArgumentSP, mode);
            }
        }

        private static bool TryDeleteMethodFromDatabase(SerializedProperty parameterInfoSP, SerializedProperty customArgumentSP)
        {
            var methodIdSP = customArgumentSP.FindPropertyRelative(AUEUtils.MethodIdSPName);
            if (methodIdSP == null)
            {
                return false;
            }

            var aueRoot = AUEUtils.FindAUERootInParent(customArgumentSP);
            byte methodId = (byte)methodIdSP.intValue;
            AUEMethodDatabaseUtils.DeleteEntry(aueRoot, methodId);
            return true;
        }
    }
}