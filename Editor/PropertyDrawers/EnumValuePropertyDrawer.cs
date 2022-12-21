using System;
using TypeCodebase;
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

            var valueSP = property.FindPropertyRelative(ValueSPName);
            _enumValue = (Enum) Enum.ToObject(_cachedType, valueSP.intValue);

            Enum newValue;

            if (isFlag)
            {
                newValue = EditorGUI.EnumFlagsField(position, label, _enumValue);
            }
            else
            {
                newValue = EditorGUI.EnumPopup(position, label, _enumValue);
            }

            if (newValue != _enumValue)
            {
                valueSP.intValue = (int)(object)newValue;
            }
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
