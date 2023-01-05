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
        private class PropertyInfoCache
        {
            public AUECAProperty TargetProperty;
            public List<MemberInfo> PropertyPathItems;

            public SerializedProperty SourceModeSP;
            public SerializedProperty ExecutionSafeModeSP;
            public SerializedProperty TargetSP;
            public SerializedProperty ArgIndexSP;
            public SerializedProperty PropertyPathSP;
            public string[] SourceOptions;
            public GUIContent[] ExecutionSafeModeOptions;
            public Type[] ArgTypes;

            public Vector2 ScrollViewPosition;
            public bool RequiresScrollView;
            public string[] PropertyPath;
        }

        public const string SourceModeSPName = "_sourceMode";
        public const string ExecutionSafeModeSPName = "_executionSafeMode";
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

        private GUIStyle _actionLabelStyle;
        private Dictionary<string, PropertyInfoCache> _propInfoCacheDb = new Dictionary<string, PropertyInfoCache>();

        public static void Initialize(SerializedProperty property)
        {
        }

        private PropertyInfoCache CacheSerializedProperty(SerializedProperty property)
        {
            if (!_propInfoCacheDb.TryGetValue(property.propertyPath, out PropertyInfoCache pic))
            {
                pic = new PropertyInfoCache()
                {
                    TargetProperty = property.GetTarget<AUECAProperty>(),
                    SourceModeSP = property.FindPropertyRelative(SourceModeSPName),
                    ExecutionSafeModeSP = property.FindPropertyRelative(ExecutionSafeModeSPName),
                    TargetSP = property.FindPropertyRelative(TargetSPName),
                    ArgIndexSP = property.FindPropertyRelative(ArgIndexSPName),
                    PropertyPathSP = property.FindPropertyRelative(PropertyPathSPName),
                    ExecutionSafeModeOptions = EnumDisplayNameHelper.BuildEnumOptions<AUECAProperty.EExecutionSafeMode>(),
                };

                pic.PropertyPath = (!string.IsNullOrEmpty(pic.PropertyPathSP.stringValue) ? pic.PropertyPathSP.stringValue.Split('.') : new string[0]);

                Type[] argTypes = AUEUtils.LoadMethodDynamicParameterTypes(property);
                if (argTypes != null)
                {
                    pic.SourceOptions = new string[ReservedOptionsCount + argTypes.Length];
                    pic.SourceOptions[TargetOptionIndex] = "Target";
                    for (int i = 0; i < argTypes.Length; ++i)
                    {
                        pic.SourceOptions[i + ReservedOptionsCount] = $"{argTypes[i].Name} arg{i}";
                    }
                }
                pic.ArgTypes = argTypes;

                if (pic.PropertyPathItems == null)
                {
                    pic.PropertyPathItems = BuildPropertyPathItems(pic);
                }

                _propInfoCacheDb.Add(property.propertyPath, pic);
            }
            _actionLabelStyle = GUI.skin.button;
            return pic;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            PropertyInfoCache pic = CacheSerializedProperty(property);

            float height = EditorGUIUtility.singleLineHeight;
            if (pic.RequiresScrollView)
            {
                height += EditorGUIUtility.singleLineHeight;
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PropertyInfoCache pic = CacheSerializedProperty(property);

            float scrollViewWidth = GetScrollViewWidth(pic);
            if (position.width > 0)
            {
                bool newRequiresScrollView = (scrollViewWidth > position.width);
                if (newRequiresScrollView != pic.RequiresScrollView)
                {
                    pic.RequiresScrollView = newRequiresScrollView;
                    RepaintProperty(property);
                }
            }

            if (pic.RequiresScrollView)
            {
                Rect viewRect = new Rect(0, 0, scrollViewWidth, EditorGUIUtility.singleLineHeight);
                pic.ScrollViewPosition = GUI.BeginScrollView(position, pic.ScrollViewPosition, viewRect, alwaysShowHorizontal: true, alwaysShowVertical: false);
                position = viewRect;
            }
            else
            {
                position.height = EditorGUIUtility.singleLineHeight;
            }

            DrawExecutionSafeMode(pic, ref position);
            DrawSourceOptions(pic, ref position);
            DrawTargetIFN(pic, ref position);
            DrawPropertyPath(pic, ref position, property);

            if (pic.RequiresScrollView)
            {
                GUI.EndScrollView();
            }
        }

        private float GetScrollViewWidth(PropertyInfoCache pic)
        {
            GUIContent content = new GUIContent();

            float width = 0.0f;

            // Execution safe mode width
            int executionModeOptionIndex = pic.ExecutionSafeModeSP.enumValueIndex;
            content.text = pic.ExecutionSafeModeOptions[executionModeOptionIndex].text;
            width = _actionLabelStyle.CalcSize(content).x + 30 + Space;

            // Source Mode width
            int sourceOptionIndex = FindSourceOptionIndex(pic);
            content.text = pic.SourceOptions[sourceOptionIndex];
            width += _actionLabelStyle.CalcSize(content).x + 30 + Space;

            // Property path width
            if (pic.PropertyPathItems != null)
            {
                for (int i = 0; i < pic.PropertyPathItems.Count; ++i)
                {
                    content.text = pic.PropertyPathItems[i]?.Name ?? NoneFieldLabel;
                    width += _actionLabelStyle.CalcSize(content).x;
                    if (i + 1 < pic.PropertyPathItems.Count)
                    {
                        width += Space;
                    }

                }

                // Actions width
                width += PropertyPathActionButtonWidth * 2 + Space;
            }

            return width;
        }

        private Rect DrawExecutionSafeMode(PropertyInfoCache pic, ref Rect position)
        {
            position.width = _actionLabelStyle.CalcSize(pic.ExecutionSafeModeOptions[pic.ExecutionSafeModeSP.enumValueIndex]).x + 30;

            pic.ExecutionSafeModeSP.enumValueIndex = 
                EditorGUI.Popup(position, GUIContent.none, pic.ExecutionSafeModeSP.enumValueIndex, pic.ExecutionSafeModeOptions);

            position.x += position.width - 15 + Space;
            return position;
        }

        private Rect DrawSourceOptions(PropertyInfoCache pic, ref Rect position)
        {
            int sourceOptionIndex = FindSourceOptionIndex(pic);

            position.width = _actionLabelStyle.CalcSize(new GUIContent(pic.SourceOptions[sourceOptionIndex])).x + 30;

            int selectedSourceOption = EditorGUI.Popup(position, sourceOptionIndex, pic.SourceOptions);
            if (sourceOptionIndex != selectedSourceOption)
            {
                ApplySourceOption(pic, selectedSourceOption);
            }
            position.x += position.width + Space;
            return position;
        }

        private Rect DrawTargetIFN(PropertyInfoCache pic, ref Rect position)
        {
            if (IsTargetModeUsed(pic))
            {
                float spaceFixerWidth = 15; // Use to counterbalanced an empty space created by the ObjectField
                position.width = TargetWidth + spaceFixerWidth * 2;
                position.x -= spaceFixerWidth;
                
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUI.PropertyField(position, pic.TargetSP, GUIContent.none);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    pic.PropertyPathItems.Clear();
                    SetDirty(pic);
                }

                position.x += position.width + Space;
                position.width = TargetWidth;
            }
            return position;
        }

        private void ApplySourceOption(PropertyInfoCache pic, int selectedSourceOption)
        {
            if (selectedSourceOption == TargetOptionIndex)
            {
                pic.SourceModeSP.enumValueIndex = (int)AUECAProperty.ESourceMode.Target;
                pic.ArgIndexSP.intValue = -1;
            }
            else
            {
                pic.SourceModeSP.enumValueIndex = (int)AUECAProperty.ESourceMode.Argument;
                pic.ArgIndexSP.intValue = selectedSourceOption - ReservedOptionsCount;
            }
            SetDirty(pic);
        }

        private int FindSourceOptionIndex(PropertyInfoCache pic)
        {
            var sourceMode = (AUECAProperty.ESourceMode)pic.SourceModeSP.enumValueIndex;
            if (IsTargetModeUsed(pic))
            {
                return TargetOptionIndex;
            }
            else if (sourceMode == AUECAProperty.ESourceMode.Argument && pic.ArgTypes != null)
            {
                int argIdx = pic.ArgIndexSP.intValue;
                if (argIdx >= 0 && argIdx < pic.ArgTypes.Length)
                {
                    return ReservedOptionsCount + argIdx;
                }
            }
            return 0;
        }

        private bool IsTargetModeUsed(PropertyInfoCache pic)
        {
            var sourceMode = (AUECAProperty.ESourceMode)pic.SourceModeSP.enumValueIndex;
            return (sourceMode == AUECAProperty.ESourceMode.Target);
        }

        private void DrawPropertyPath(PropertyInfoCache pic, ref Rect position, SerializedProperty property)
        {
            if (pic.PropertyPathItems == null)
            {
                return;
            }

            Rect fieldRect = new Rect(position.x, position.y, FieldPropertyWidth, position.height);
            Type parentType = GetTargetType(pic);
            for (int i = 0; i < pic.PropertyPathItems.Count; ++i)
            {
                if (parentType != null)
                {
                    var propertyPathItem = pic.PropertyPathItems[i];
                    string name = propertyPathItem?.Name ?? NoneFieldLabel;
                    fieldRect.width = _actionLabelStyle.CalcSize(new GUIContent(name)).x;
                    if (GUI.Button(fieldRect, name))
                    {
                        new MemberInfosSearchDropdown(property, parentType, i, (sp, selected, index) =>
                        {
                            SetPropertyPathItem(pic, (int)index, selected);
                            UpdatePropertyPath(pic);
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

            DrawPropertyPathActions(pic, ref fieldRect);
        }

        private void SetPropertyPathItem(PropertyInfoCache pic, int index, MemberInfo selected)
        {
            if (GetMemberType(pic.PropertyPathItems[index]) != GetMemberType(selected))
            {
                pic.PropertyPathItems.RemoveRange(index + 1, pic.PropertyPathItems.Count - (index + 1));
            }
            pic.PropertyPathItems[index] = selected;
        }

        private void DrawPropertyPathActions(PropertyInfoCache pic, ref Rect position)
        {
            position.width = PropertyPathActionButtonWidth;
            if (GUI.Button(position, "+"))
            {
                pic.PropertyPathItems.Add(null);
                UpdatePropertyPath(pic);
            }
            position.x += position.width + Space;

            EditorGUI.BeginDisabledGroup(pic.PropertyPathItems.Count == 0);
            {
                if (GUI.Button(position, "-"))
                {
                    pic.PropertyPathItems.RemoveAt(pic.PropertyPathItems.Count - 1);
                    UpdatePropertyPath(pic);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private List<MemberInfo> BuildPropertyPathItems(PropertyInfoCache pic)
        {
            Type targetType = GetTargetType(pic);
            if (targetType == null)
            {
                return new List<MemberInfo>();
            }

            var propertyPathItems = new List<MemberInfo>(pic.PropertyPath.Length);
            for (int i = 0; i < pic.PropertyPath.Length; ++i)
            {
                var memberInfo = FindMemberInfo(targetType, pic.PropertyPath[i]);
                if (memberInfo == null)
                {
                    break;
                }

                propertyPathItems.Add(memberInfo);
                targetType = GetMemberType(memberInfo);
            }
            return propertyPathItems;
        }

        private Type GetTargetType(PropertyInfoCache pic)
        {
            Type targetType = null;
            if (IsTargetModeUsed(pic))
            {
                if (pic.TargetSP.objectReferenceValue != null)
                {
                    targetType = pic.TargetSP.objectReferenceValue.GetType();
                }
            }
            else if (pic.ArgTypes != null)
            {
                int argIdx = pic.ArgIndexSP.intValue;
                targetType = pic.ArgTypes[argIdx];
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

        private void UpdatePropertyPath(PropertyInfoCache pic)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < pic.PropertyPathItems.Count; ++i)
            {
                if (pic.PropertyPathItems[i] == null)
                {
                    break;
                }

                sb.Append(pic.PropertyPathItems[i].Name);
                if (i + 1 < pic.PropertyPathItems.Count && pic.PropertyPathItems[i + 1] != null)
                {
                    sb.Append('.');
                }
            }
            pic.PropertyPathSP.stringValue = sb.ToString();
            SetDirty(pic);
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

        private void SetDirty(PropertyInfoCache pic) => pic.TargetProperty.SetDirty();
    }
}

