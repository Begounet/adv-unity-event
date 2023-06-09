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
        private const int ModeWidth = 150;
        private const int LabelModeSpace = 10;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded)
            {
                var customMethodSP = property.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
                height += EditorGUI.GetPropertyHeight(customMethodSP, customMethodSP.isExpanded);
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect lineRect = position;
            lineRect.height = EditorGUIUtility.singleLineHeight;

            Rect labelRect = new Rect(lineRect.x, lineRect.y, lineRect.width - ModeWidth - LabelModeSpace, lineRect.height);
            Rect modeRect = new Rect(lineRect.xMax - ModeWidth, lineRect.y, ModeWidth, lineRect.height);
            property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label);

            var customArgumentSP = property.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
            var modeSP = property.FindPropertyRelative(AUEUtils.ModeSPName);
            InitializeCustomArgumentIFN(customArgumentSP, (EMode)modeSP.enumValueIndex);

            EditorGUI.BeginChangeCheck();
            {
                using (new PropertyDrawerHelper.IndentedLevelResetScope())
                {
                    EditorGUI.PropertyField(modeRect, modeSP, GUIContent.none);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                UpdateCustomArgumentType(property, customArgumentSP, (EMode)modeSP.enumValueIndex);
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            if (property.isExpanded)
            {
                lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                ++EditorGUI.indentLevel;
                {
                    Rect propRect = new Rect(lineRect.x, lineRect.y, lineRect.width, EditorGUI.GetPropertyHeight(customArgumentSP, includeChildren: customArgumentSP.isExpanded));
                    var propertyDrawer = customArgumentSP.GetPropertyDrawer();
                    propertyDrawer.OnGUI(propRect, customArgumentSP, new GUIContent(customArgumentSP.displayName));
                }
                --EditorGUI.indentLevel;
            }
        }

        public static void InitializeCustomArgument(SerializedProperty parameterInfoSP)
        {
            var customArgumentSP = parameterInfoSP.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
            var modeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ModeSPName);
            UpdateCustomArgumentType(parameterInfoSP, customArgumentSP, (EMode)modeSP.intValue);
        }

        private static void UpdateCustomArgumentType(SerializedProperty parameterInfoSP, SerializedProperty customArgumentSP, EMode mode)
        {
            TryDeleteMethodFromDatabase(parameterInfoSP, customArgumentSP);
            InitializeCustomArgument(customArgumentSP, mode);
        }

        private static void InitializeCustomArgumentIFN(SerializedProperty customArgumentSP, EMode mode)
        {
            if (string.IsNullOrEmpty(customArgumentSP.managedReferenceFullTypename))
            {
                InitializeCustomArgument(customArgumentSP, mode);
            }
        }

        private static void InitializeCustomArgument(SerializedProperty customArgumentSP, EMode mode)
        {
            object refValue = null;
            Type customArgumentType = GetArgumentTypeFromMode(mode);
            if (customArgumentType != null)
            {
                refValue = Activator.CreateInstance(customArgumentType);
            }
            customArgumentSP.managedReferenceValue = refValue;
            switch (mode)
            {
                case EMode.Dynamic:
                    AUECADynamicPropertyDrawer.Initialize(customArgumentSP);
                    break;
                case EMode.Constant:
                    AUECAConstantPropertyDrawer.Initialize(customArgumentSP);
                    break;
                case EMode.Method:
                    AUECAMethodReferencePropertyDrawer.Initialize(customArgumentSP);
                    break;
                case EMode.Property:
                    AUECAPropertyPropertyDrawer.Initialize(customArgumentSP);
                    break;
            }
        }

        public static Type GetArgumentTypeFromMode(EMode mode)
        {
            switch (mode)
            {
                case EMode.Dynamic:
                    return typeof(AUECADynamic);
                case EMode.Constant:
                    return typeof(AUECAConstant);
                case EMode.Property:
                    return typeof(AUECAProperty);
                case EMode.Method:
                    return typeof(AUECAMethodReference);
            }
            return null;
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