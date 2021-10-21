using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECADynamic))]
    public class AUECADynamicPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type[] argumentTypes = LoadDynamicTypeFromParent(property);

            if (argumentTypes.Length == 0)
            {
                DrawNoArguments(position);
            }
            else
            {
                DrawArgumentsSelection(position, argumentTypes, property);
            }
        }

        private void DrawNoArguments(Rect position)
        {
            EditorGUI.LabelField(position, "No argument supplied. Cannot use Dynamic mode.");
        }

        private void DrawArgumentsSelection(Rect position, Type[] argumentTypes, SerializedProperty property)
        {
            Rect argumentRect = position;
            argumentRect.width /= argumentTypes.Length;

            var paramInfoSP = property.GetParent();
            var paramInfoTypeSP = paramInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
            var paramInfoType = SerializableTypeHelper.LoadType(paramInfoTypeSP);

            var sourceArgumentIndexSP = property.FindPropertyRelative(AUEUtils.CADynamicSourceArgumentIndexSPName);
            int oldArgumentIndex = sourceArgumentIndexSP.intValue;
            int newArgumentIndex = oldArgumentIndex;
            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                bool doesMethodParameterMatchArgumentType = DoesMethodParameterMatchArgumentType(paramInfoType, argumentTypes[i]);
                EditorGUI.BeginDisabledGroup(!doesMethodParameterMatchArgumentType);
                if (EditorGUI.ToggleLeft(argumentRect, argumentTypes[i].Name, doesMethodParameterMatchArgumentType && i == newArgumentIndex))
                {
                    newArgumentIndex = i;
                }
                EditorGUI.EndDisabledGroup();
                argumentRect.x += argumentRect.width;
            }
            if (newArgumentIndex != oldArgumentIndex)
            {
                sourceArgumentIndexSP.intValue = newArgumentIndex;
            }
        }

        private bool DoesMethodParameterMatchArgumentType(Type methodParamType, Type dynamicType) 
            => (dynamicType == methodParamType || dynamicType.IsSubclassOf(methodParamType));

        private static Type[] LoadDynamicTypeFromParent(SerializedProperty property)
        {
            var aueRootSP = AUEUtils.FindAUERootInParent(property);
            var argumentTypesSP = aueRootSP.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);
            Type[] types = new Type[argumentTypesSP.arraySize];
            for (int i = 0; i < argumentTypesSP.arraySize; ++i)
            {
                var argumentTypeSP = argumentTypesSP.GetArrayElementAtIndex(i);
                types[i] = SerializableTypeHelper.LoadType(argumentTypeSP);
            }
            return types;
        }
    }
}