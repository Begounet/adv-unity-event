using System;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECAConstant))]
    public class AUECAConstantPropertyDrawer : PropertyDrawer
    {
        private const string ConstantValueSPName = "_constantValue";
        private const string ConstantInternalValueSPName = "Value";
        private const string ConstantInternalValueDisplayName = "Value";

        private bool _isInitialized = false;
        private GUIContent _internalLabelValue;
        private SerializedProperty _constantTypeSP;
        private SerializedProperty _constantValueSP;
        private SerializedProperty _internValueSP;
        private SerializedProperty _parentParameterTypeSP;
        private Type _argumentType;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize();
            CacheDataThisFrame(property);
            EnsureConstantTypeMatching();
            EnsureConstantValueValidity(property);

            if (!CanBeDrawn())
            {
                return 0.0f;
            }

            if (!string.IsNullOrEmpty(_constantValueSP.managedReferenceFullTypename))
            {
                return EditorGUI.GetPropertyHeight(_internValueSP, _internValueSP.isExpanded) + EditorGUIUtility.standardVerticalSpacing;
            }
            return 0.0f;
        }

        private bool CanBeDrawn() => (_argumentType != null && _internValueSP != null);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize();
            CacheDataThisFrame(property);
            EnsureConstantTypeMatching();
            EnsureConstantValueValidity(property);

            if (!CanBeDrawn())
            {
                return;
            }

            position.height = EditorGUIUtility.singleLineHeight;

            // Special case for UnityEngine.Object. They are saved as UnityEngine.Object but
            // we want to constrain this object field to the expected parameter type.
            if (typeof(UnityEngine.Object).IsAssignableFrom(_argumentType))
            {
                _internValueSP.objectReferenceValue =
                    EditorGUI.ObjectField(
                        position,
                        _internalLabelValue,
                        _internValueSP.objectReferenceValue,
                        _argumentType,
                        allowSceneObjects: true);
            }
            else
            {
                EditorGUI.PropertyField(position, _internValueSP, _internalLabelValue, _internValueSP.isExpanded);
            }
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _internalLabelValue = new GUIContent(ConstantInternalValueDisplayName);
            _isInitialized = true;
        }

        private void CacheDataThisFrame(SerializedProperty property)
        {
            _constantTypeSP = property.FindPropertyRelative(AUEUtils.CAConstantTypeSPName);
            _argumentType = SerializableTypeHelper.LoadType(_constantTypeSP);
            _constantValueSP = property.FindPropertyRelative(ConstantValueSPName);
            _internValueSP = _constantValueSP.FindPropertyRelative(ConstantInternalValueSPName);

            var parameterInfoSP = property.GetParent();
            _parentParameterTypeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
        }

        private void EnsureConstantTypeMatching()
        {
            SerializableTypeHelper.CopySerializableType(_parentParameterTypeSP, _constantTypeSP);
        }

        public static object GetDefault(Type t)
        {
            try
            {
                if (t == typeof(string))
                {
                    return string.Empty;
                }
                return Activator.CreateInstance(t);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        private void EnsureConstantValueValidity(SerializedProperty property)
        {
            if (_argumentType != null && string.IsNullOrEmpty(_constantValueSP.managedReferenceFullTypename))
            {
                _constantValueSP.managedReferenceValue = TryCreateDefaultManagedReferenceValue(_argumentType);
            }
        }

        private IConstantValue TryCreateDefaultManagedReferenceValue(Type argumentType)
        {
            try
            {
                foreach (var constantValue in StandardConstantValues.ConstantMapping)
                {
                    if (constantValue.Key.IsAssignableFrom(argumentType))
                    {
                        return (IConstantValue)Activator.CreateInstance(constantValue.Value);
                    }
                }

                var genericObject = new StandardConstantValues.GenericObject();
                genericObject.Value = Activator.CreateInstance(argumentType);
                return genericObject;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }
    }
}