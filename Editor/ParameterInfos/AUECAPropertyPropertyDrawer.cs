using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECAProperty))]
    public class AUECAPropertyPropertyDrawer : PropertyDrawer
    {
        private const string SourceModeSPName = "_sourceMode";
        private const string TargetSPName = "_target";
        private const string ArgIndexSPName = "_argIndex";
        private const string PropertyPathSPName = "_propertyPath";

        private const float SourceModeWidth = 120;
        private const float Space = 2;
        private const float PointWidth = 2;

        private const int ReservedOptionsCount = 1;
        private const int TargetOptionIndex = 0;

        private SerializedProperty _sourceModeSP;
        private SerializedProperty _targetSP;
        private SerializedProperty _argIndexSP;
        private SerializedProperty _propertyPathSP;
        private string[] _sourceOptions;
        private ParameterInfo[] _methodParameters;

        private Vector2 scrollViewPosition;

        public static void Initialize(SerializedProperty property)
        {
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CacheSerializedProperty(property);

            float height = EditorGUIUtility.singleLineHeight;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheSerializedProperty(property);

            int columnCount = IsTargetModeUsed() ? 3 : 2;
            float columnWidth = (position.width - (PointWidth + Space * 2)) / columnCount - (Space * columnCount - 1);

            position.width = columnWidth;

            DrawSourceOptions(ref position);
            DrawTargetIFN(ref position);

            Rect pointRect = new Rect(position.x - 15, position.y, PointWidth, position.height);
            EditorGUI.LabelField(pointRect, ".", string.Empty);
            position.x += pointRect.width + Space;

            string propertyPath = _propertyPathSP.stringValue;
            propertyPath = EditorGUI.TextField(position, GUIContent.none, propertyPath);
            if (string.IsNullOrEmpty(propertyPath))
            {
                var guiColor = GUI.color;
                GUI.color = Color.grey;
                EditorGUI.LabelField(position, string.Empty, "Property1.Property2");
                GUI.color = guiColor;
            }
            _propertyPathSP.stringValue = propertyPath;
        }

        private Rect DrawSourceOptions(ref Rect position)
        {
            int sourceOptionIndex = FindSourceOptionIndex();
            int selectedSourceOption = EditorGUI.Popup(position, sourceOptionIndex, _sourceOptions);
            if (sourceOptionIndex != selectedSourceOption)
            {
                ApplySourceOption(selectedSourceOption);
            }

            position.x += position.width + Space;
            return position;
        }

        private Rect DrawTargetIFN(ref Rect position)
        {
            if (IsTargetModeUsed())
            {
                EditorGUI.PropertyField(position, _targetSP, GUIContent.none);
                position.x += position.width + Space;
            }
            return position;
        }

        private void ApplySourceOption(int selectedSourceOption)
        {
            if (selectedSourceOption == TargetOptionIndex)
            {
                _sourceModeSP.enumValueIndex = (int) AUECAProperty.ESourceMode.Target;
                _argIndexSP.intValue = -1;
            }
            else
            {
                _sourceModeSP.enumValueIndex = (int)AUECAProperty.ESourceMode.Argument;
                _argIndexSP.intValue = selectedSourceOption - ReservedOptionsCount;
            }
        }

        private int FindSourceOptionIndex()
        {
            if (IsTargetModeUsed())
            {
                return TargetOptionIndex;
            }
            else // if (sourceMode == AUECAProperty.ESourceMode.Argument)
            {
                int argIdx = _argIndexSP.intValue;
                if (argIdx >= 0 && argIdx < _methodParameters.Length)
                {
                    return ReservedOptionsCount + argIdx;
                }
            }
            return 0;
        }

        private void CacheSerializedProperty(SerializedProperty property)
        {
            _sourceModeSP = property.FindPropertyRelative(SourceModeSPName);
            _targetSP = property.FindPropertyRelative(TargetSPName);
            _argIndexSP = property.FindPropertyRelative(ArgIndexSPName);
            _propertyPathSP = property.FindPropertyRelative(PropertyPathSPName);

            var aueMethodSP = AUEUtils.FindRootAUEMethod(property);
            MethodInfo aueMethodMI = AUEUtils.GetMethodInfoFromAUEMethod(aueMethodSP);
            if (aueMethodMI != null)
            {
                _methodParameters = aueMethodMI.GetParameters();
                _sourceOptions = new string[ReservedOptionsCount + _methodParameters.Length];
                _sourceOptions[TargetOptionIndex] = "Target";
                for (int i = 0; i < _methodParameters.Length; ++i)
                {
                    _sourceOptions[i + ReservedOptionsCount] = _methodParameters[i].Name;
                }
            }
            else
            {
                _methodParameters = null;
            }
        }

        private bool IsTargetModeUsed()
        {
            var sourceMode = (AUECAProperty.ESourceMode)_sourceModeSP.enumValueIndex;
            return (sourceMode == AUECAProperty.ESourceMode.Target);
        }

        //private Rect GetViewRect(Rect position, SerializedProperty property)
        //{
        //    Rect viewRect = position;
        //    viewRect.width = SourceModeWidth + Space + ;
        //}
    }
}

