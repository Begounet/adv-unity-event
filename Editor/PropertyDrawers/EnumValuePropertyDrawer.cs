using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(StandardConstantValues.EnumValue))]
    public class EnumValuePropertyDrawer : PropertyDrawer
    {
        private const string ValueSPName = "_value";
        private Enum _enumValue;
        private Type _cachedType;
        private bool _isFlag;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isFlag = Initialize(property);

            if (isFlag)
            {
                _enumValue = EditorGUI.EnumFlagsField(position, label, _enumValue);
            }
            else
            {
                _enumValue = EditorGUI.EnumPopup(position, label, _enumValue);
            }

            var valueSP = property.FindPropertyRelative(ValueSPName);
            valueSP.intValue = (int)(object) _enumValue;
        }

        private bool Initialize(SerializedProperty property)
        {
            if (_cachedType != null)
            {
                return _isFlag;
            }

            var constantSP = property.GetParent();
            var constantTypeSP = constantSP.FindPropertyRelative(AUEUtils.CAConstantTypeSPName);
            _cachedType = SerializableTypeHelper.LoadType(constantTypeSP);
            object[] attributes = _cachedType.GetCustomAttributes(typeof(FlagsAttribute), inherit: true);
            _isFlag = (attributes.Length > 0);

            if (_enumValue == null)
            {
                _enumValue = (Enum)Activator.CreateInstance(_cachedType);
            }

            return _isFlag;
        }
    }
}
