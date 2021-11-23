using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECAProperty))]
    public class AUECAPropertyPropertyDrawer : PropertyDrawer
    {
        public const string SourceModeSPName = "_sourceMode";
        public const string TargetSPName = "_target";
        public const string ArgIndexSPName = "_argIndex";
        public const string PropertyPathSPName = "_propertyPath";

        private const float SourceModeWidth = 100;
        private const float TargetWidth = 120;
        private const float FieldPropertyWidth = 100;
        private const float PropertyPathActionButtonWidth = 20;
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

        private Vector2 _scrollViewPosition;
        private bool _requiresScrollView;
        private string[] _propertyPath;

        private List<MemberInfo> _propertyPathItems;

        public static void Initialize(SerializedProperty property)
        {
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CacheSerializedProperty(property);

            float height = EditorGUIUtility.singleLineHeight;
            if (_requiresScrollView)
            {
                height += 10;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheSerializedProperty(property);

            float scrollViewWidth = GetScrollViewWidth();
            _requiresScrollView = (scrollViewWidth > position.width);

            if (_requiresScrollView)
            {
                Rect viewRect = new Rect(0, 0, scrollViewWidth, EditorGUIUtility.singleLineHeight);
                _scrollViewPosition = GUI.BeginScrollView(position, _scrollViewPosition, viewRect, alwaysShowHorizontal: true, alwaysShowVertical: false);
                position = viewRect;
            }
            else
            {
                position.height = EditorGUIUtility.singleLineHeight;
            }
            {
                DrawSourceOptions(ref position);
                DrawTargetIFN(ref position);
                DrawPropertyPath(ref position, property);
            }
            if (_requiresScrollView)
            {
                GUI.EndScrollView();
            }
        }

        private float GetScrollViewWidth()
        {
            float width = SourceModeWidth + Space;
            if (IsTargetModeUsed())
            {
                width += TargetWidth + Space;
            }
            width += PointWidth + Space;
            {
                width += _propertyPath.Length * FieldPropertyWidth;
                width += Space * (_propertyPath.Length - 1);
            }
            return width;
        }

        private Rect DrawSourceOptions(ref Rect position)
        {
            position.width = SourceModeWidth;
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
            position.width = TargetWidth;
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
            _propertyPath = (!string.IsNullOrEmpty(_propertyPathSP.stringValue) ? _propertyPathSP.stringValue.Split('.') : new string[0]);

            if (_propertyPathItems == null)
            {
                _propertyPathItems = BuildPropertyPathItems();
            }

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

        private void DrawPropertyPath(ref Rect position, SerializedProperty property)
        {
            Rect fieldRect = new Rect(position.x, position.y, FieldPropertyWidth, position.height);
            Type parentType = GetTargetType();
            for (int i = 0; i < _propertyPathItems.Count; ++i)
            {
                if (parentType != null)
                {
                    var propertyPathItem = _propertyPathItems[i];
                    string name = propertyPathItem?.Name ?? "None";
                    if (GUI.Button(fieldRect, name))
                    {
                        new MemberInfosSearchDropdown(property, parentType, i, (sp, selected, index) =>
                        {
                            SetPropertyPathItem((int)index, selected);
                            UpdatePropertyPath();
                            property.serializedObject.ApplyModifiedProperties();
                        }).Show(fieldRect);
                    }
                    fieldRect.x += fieldRect.width + Space;
                    if (propertyPathItem != null)
                    {
                        parentType = GetMemberType(propertyPathItem);
                    }
                }
            }

            DrawPropertyPathActions(ref fieldRect);
        }

        private void SetPropertyPathItem(int index, MemberInfo selected)
        {
            if (GetMemberType(_propertyPathItems[index]) != GetMemberType(selected))
            {
                _propertyPathItems.RemoveRange(index + 1, _propertyPathItems.Count - (index + 1));
            }
            _propertyPathItems[index] = selected;
        }

        private void DrawPropertyPathActions(ref Rect position)
        {
            position.width = PropertyPathActionButtonWidth;
            if (GUI.Button(position, "+"))
            {
                _propertyPathItems.Add(null);
                UpdatePropertyPath();
            }
            position.x += position.width + Space;
            if (GUI.Button(position, "-"))
            {
                _propertyPathItems.RemoveAt(_propertyPathItems.Count - 1);
                UpdatePropertyPath();
            }
        }

        private List<MemberInfo> BuildPropertyPathItems()
        {
            Type targetType = GetTargetType();
            if (targetType == null)
            {
                return null;
            }

            var propertyPathItems = new List<MemberInfo>(_propertyPath.Length);
            for (int i = 0; i < _propertyPath.Length; ++i)
            {
                var memberInfo = FindMemberInfo(targetType, _propertyPath[i]);
                if (memberInfo == null)
                {
                    break;
                }

                propertyPathItems.Add(memberInfo);
                targetType = GetMemberType(memberInfo);
            }
            return propertyPathItems;
        }

        private Type GetTargetType()
        {
            Type targetType = null;
            if (IsTargetModeUsed())
            {
                if (_targetSP.objectReferenceValue != null)
                {
                    targetType = _targetSP.objectReferenceValue.GetType();
                }
            }
            else
            {
                int argIdx = _argIndexSP.intValue;
                targetType = _methodParameters[argIdx].ParameterType;
            }
            return targetType;
        }

        private MemberInfo FindMemberInfo(Type targetType, string memberName)
        {
            MemberInfo[] mis = MemberInfoCache.GetMemberInfos(targetType);
            for (int i = 0; i < mis.Length; ++i)
            {
                if (mis[i].Name == memberName)
                {
                    return mis[i];
                }
            }
            return null;
        }

        private Type GetMemberType(MemberInfo mi)
        {
            if (mi is PropertyInfo pi)
            {
                return pi.PropertyType;
            }
            else if (mi is FieldInfo fi)
            {
                return fi.FieldType;
            }
            return null;
        }

        private void UpdatePropertyPath()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _propertyPathItems.Count; ++i)
            {
                if (_propertyPathItems[i] == null)
                {
                    break;
                }

                sb.Append(_propertyPathItems[i].Name);
                if (i + 1 < _propertyPathItems.Count && _propertyPathItems[i + 1] != null)
                {
                    sb.Append('.');
                }
            }
            _propertyPathSP.stringValue = sb.ToString();
        }
    }
}

