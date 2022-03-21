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

        private const float TargetWidth = 150;
        private const float FieldPropertyWidth = 100;
        private const float PropertyPathActionButtonWidth = 20;
        private const float Space = 2;

        private const string NoneFieldLabel = "None";

        private const int ReservedOptionsCount = 1;
        private const int TargetOptionIndex = 0;

        private SerializedProperty _sourceModeSP;
        private SerializedProperty _targetSP;
        private SerializedProperty _argIndexSP;
        private SerializedProperty _propertyPathSP;
        private string[] _sourceOptions;
        private Type[] _argTypes;

        private GUIStyle _actionLabelStyle;

        private Vector2 _scrollViewPosition;
        private bool _requiresScrollView;
        private string[] _propertyPath;

        private AUECAProperty _targetProperty;
        private List<MemberInfo> _propertyPathItems;

        public static void Initialize(SerializedProperty property)
        {
        }

        private void CacheSerializedProperty(SerializedProperty property)
        {
            _targetProperty = property.GetTarget<AUECAProperty>();

            _sourceModeSP = property.FindPropertyRelative(SourceModeSPName);
            _targetSP = property.FindPropertyRelative(TargetSPName);
            _argIndexSP = property.FindPropertyRelative(ArgIndexSPName);
            _propertyPathSP = property.FindPropertyRelative(PropertyPathSPName);
            _propertyPath = (!string.IsNullOrEmpty(_propertyPathSP.stringValue) ? _propertyPathSP.stringValue.Split('.') : new string[0]);

            if (_propertyPathItems == null)
            {
                _propertyPathItems = BuildPropertyPathItems();
            }

            _argTypes = AUEUtils.LoadMethodDynamicParameterTypes(property);
            if (_argTypes != null)
            {
                _sourceOptions = new string[ReservedOptionsCount + _argTypes.Length];
                _sourceOptions[TargetOptionIndex] = "Target";
                for (int i = 0; i < _argTypes.Length; ++i)
                {
                    _sourceOptions[i + ReservedOptionsCount] = $"{_argTypes[i].Name} arg{i}";
                }
            }
            else
            {
                _argTypes = null;
            }

            _actionLabelStyle = GUI.skin.button;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CacheSerializedProperty(property);

            float height = EditorGUIUtility.singleLineHeight;
            if (_requiresScrollView)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheSerializedProperty(property);

            float scrollViewWidth = GetScrollViewWidth();
            if (position.width > 0)
            {
                bool newRequiresScrollView = (scrollViewWidth > position.width);
                if (newRequiresScrollView != _requiresScrollView)
                {
                    _requiresScrollView = newRequiresScrollView;
                    RepaintProperty(property);
                }
            }

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
            GUIContent content = new GUIContent();

            int sourceOptionIndex = FindSourceOptionIndex();
            content.text = _sourceOptions[sourceOptionIndex];

            float width = 0.0f;

            // Source Mode width
            width = _actionLabelStyle.CalcSize(content).x + 30 + Space;

            // Property path width
            if (_propertyPathItems != null)
            {
                for (int i = 0; i < _propertyPathItems.Count; ++i)
                {
                    content.text = _propertyPathItems[i]?.Name ?? NoneFieldLabel;
                    width += _actionLabelStyle.CalcSize(content).x;
                    if (i + 1 < _propertyPathItems.Count)
                    {
                        width += Space;
                    }

                }

                // Actions width
                width += PropertyPathActionButtonWidth * 2 + Space;
            }

            return width;
        }

        private Rect DrawSourceOptions(ref Rect position)
        {
            int sourceOptionIndex = FindSourceOptionIndex();

            position.width = _actionLabelStyle.CalcSize(new GUIContent(_sourceOptions[sourceOptionIndex])).x + 30;

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
                float spaceFixerWidth = 15; // Use to counterbalanced an empty space created by the ObjectField
                position.width = TargetWidth + spaceFixerWidth * 2;
                position.x -= spaceFixerWidth;
                
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.PropertyField(position, _targetSP, GUIContent.none);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SetDirty();
                }

                position.x += position.width + Space;
                position.width = TargetWidth;
            }
            return position;
        }

        private void ApplySourceOption(int selectedSourceOption)
        {
            if (selectedSourceOption == TargetOptionIndex)
            {
                _sourceModeSP.enumValueIndex = (int)AUECAProperty.ESourceMode.Target;
                _argIndexSP.intValue = -1;
            }
            else
            {
                _sourceModeSP.enumValueIndex = (int)AUECAProperty.ESourceMode.Argument;
                _argIndexSP.intValue = selectedSourceOption - ReservedOptionsCount;
            }
            SetDirty();
        }

        private int FindSourceOptionIndex()
        {
            var sourceMode = (AUECAProperty.ESourceMode)_sourceModeSP.enumValueIndex;
            if (IsTargetModeUsed())
            {
                return TargetOptionIndex;
            }
            else if (sourceMode == AUECAProperty.ESourceMode.Argument && _argTypes != null)
            {
                int argIdx = _argIndexSP.intValue;
                if (argIdx >= 0 && argIdx < _argTypes.Length)
                {
                    return ReservedOptionsCount + argIdx;
                }
            }
            return 0;
        }

        private bool IsTargetModeUsed()
        {
            var sourceMode = (AUECAProperty.ESourceMode)_sourceModeSP.enumValueIndex;
            return (sourceMode == AUECAProperty.ESourceMode.Target);
        }

        private void DrawPropertyPath(ref Rect position, SerializedProperty property)
        {
            if (_propertyPathItems == null)
            {
                return;
            }

            Rect fieldRect = new Rect(position.x, position.y, FieldPropertyWidth, position.height);
            Type parentType = GetTargetType();
            for (int i = 0; i < _propertyPathItems.Count; ++i)
            {
                if (parentType != null)
                {
                    var propertyPathItem = _propertyPathItems[i];
                    string name = propertyPathItem?.Name ?? NoneFieldLabel;
                    fieldRect.width = _actionLabelStyle.CalcSize(new GUIContent(name)).x;
                    if (GUI.Button(fieldRect, name))
                    {
                        new MemberInfosSearchDropdown(property, parentType, i, (sp, selected, index) =>
                        {
                            SetPropertyPathItem((int)index, selected);
                            UpdatePropertyPath();
                            property.serializedObject.ApplyModifiedProperties();
                            GUI.changed = true;
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

            EditorGUI.BeginDisabledGroup(_propertyPathItems.Count == 0);
            {
                if (GUI.Button(position, "-"))
                {
                    _propertyPathItems.RemoveAt(_propertyPathItems.Count - 1);
                    UpdatePropertyPath();
                }
            }
            EditorGUI.EndDisabledGroup();
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
            else if (_argTypes != null)
            {
                int argIdx = _argIndexSP.intValue;
                targetType = _argTypes[argIdx];
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
            SetDirty();
        }

        private void RepaintProperty(SerializedProperty property)
        {
            foreach (var item in ActiveEditorTracker.sharedTracker.activeEditors)
            {
                if (item.serializedObject == property.serializedObject)
                {
                    item.Repaint();
                    return;
                }
            }
        }

        private void SetDirty() => _targetProperty.SetDirty();
    }
}

