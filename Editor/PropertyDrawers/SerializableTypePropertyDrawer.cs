using System;
using System.Collections;
using System.Collections.Generic;
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeTypeSearchDropdownIFN(property);
            Rect valueRect = EditorGUI.PrefixLabel(position, label);
            Type currentType = SerializableTypeHelper.LoadType(property);
            if (GUI.Button(valueRect, currentType.FullName))
            {
                _typeSearchDropdown.Show(valueRect);
            }

            CheckNewTypeSelected(property);
        }

        private void CheckNewTypeSelected(SerializedProperty property)
        {
            if (!_hasNewTypeSelected)
            {
                return;
            }

            var typeFullNameSP = property.FindPropertyRelative(SerializableTypeHelper.SerializedTypeFullNameSPName);
            typeFullNameSP.stringValue = _newTypeSelected.AssemblyQualifiedName;
            _hasNewTypeSelected = false;
        }

        private void InitializeTypeSearchDropdownIFN(SerializedProperty property)
        {
            if (_typeSearchDropdown == null)
            {
                _typeSearchDropdown = new TypeSearchDropdown(_typeSearchDropdownState);
                _typeSearchDropdown.OnTypeSelected += OnTypeSelected;
            }
        }

        private void OnTypeSelected(Type newType)
        {
            _hasNewTypeSelected = true;
            _newTypeSelected = newType;
        }
    }
}
