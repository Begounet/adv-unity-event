using System;
using TypeCodebase;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(SerializableType), useForChildren: true)]
    public class SerializableTypePropertyDrawer : PropertyDrawer
    {
        [SerializeField]
        private AdvancedDropdownState _typeSearchDropdownState = new AdvancedDropdownState();

        private TypeSearchDropdown _typeSearchDropdown = null;

        private bool _hasNewTypeSelected = false;
        private Type _newTypeSelected;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeTypeSearchDropdownIFN(property);
            Rect valueRect = EditorGUI.PrefixLabel(position, label);
            Type currentType = SerializableTypeHelper.LoadType(property);

            TypeSelectorGUI.Draw(valueRect, currentType, new TypeSelectorAdvancedDropdown.Settings()
            { 
                UsageFlags = TypeCodebase.ETypeUsageFlag.Class | 
                TypeCodebase.ETypeUsageFlag.Abstract | 
                TypeCodebase.ETypeUsageFlag.Interface | 
                TypeCodebase.ETypeUsageFlag.Struct 
            }, out _hasNewTypeSelected, out _newTypeSelected);

            CheckNewTypeSelected(property);
        }

        private void CheckNewTypeSelected(SerializedProperty property)
        {
            if (!_hasNewTypeSelected)
            {
                return;
            }

            var typeFullNameSP = property.FindPropertyRelative(SerializableTypeHelper.SerializedTypeFullNameSPName);
            typeFullNameSP.stringValue = _newTypeSelected?.AssemblyQualifiedName ?? string.Empty;
            _hasNewTypeSelected = false;
        }

        private void InitializeTypeSearchDropdownIFN(SerializedProperty property)
        {
            if (_typeSearchDropdown == null)
            {
                SerializableTypeConstraintAttribute attr = GetConstraintType();
                TypeSearchDropdown.Settings settings = null;
                if (attr != null)
                {
                    settings = new TypeSearchDropdown.Settings()
                    {
                        ConstraintType = attr.ConstraintType,
                        UsageFlags = attr.FilterFlags
                    };
                }

                _typeSearchDropdown = new TypeSearchDropdown(_typeSearchDropdownState, settings);
                _typeSearchDropdown.OnTypeSelected += OnTypeSelected;
            }
        }

        private SerializableTypeConstraintAttribute GetConstraintType()
        {
            object[] constraints = fieldInfo.GetCustomAttributes(typeof(SerializableTypeConstraintAttribute), inherit: true);
            if (constraints.Length == 0)
            {
                return null;
            }

            return (constraints[0] as SerializableTypeConstraintAttribute);
        }

        private void OnTypeSelected(Type newType)
        {
            _hasNewTypeSelected = true;
            _newTypeSelected = newType;
        }
    }
}
