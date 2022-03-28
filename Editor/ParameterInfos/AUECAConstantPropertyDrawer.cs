using System;
using TypeCodebase;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECAConstant))]
    public class AUECAConstantPropertyDrawer : PropertyDrawer
    {
        public const string ConstantValueSPName = "_constantValue";
        public const string ConstantInternalValueSPName = "_value";
        public const string ConstantInternalValueDisplayName = "Value";

        private bool _isInitialized = false;
        private GUIContent _internalLabelValue;
        private SerializedProperty _constantTypeSP;
        private SerializedProperty _constantValueSP;
        private SerializedProperty _internValueSP;
        private SerializedProperty _parentParameterTypeSP;
        private Type _argumentType;
        private bool _shouldDrawOnlyValue;

        public static void Initialize(SerializedProperty property)
        {
            var parameterInfoSP = property.GetParent();
            var parentParameterTypeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
            var constantTypeSP = property.FindPropertyRelative(AUEUtils.CAConstantTypeSPName);
            SerializableTypeHelper.CopySerializableType(parentParameterTypeSP, constantTypeSP);

            var argumentType = SerializableTypeHelper.LoadType(constantTypeSP);
            var constantValueSP = property.FindPropertyRelative(ConstantValueSPName);

            if (argumentType != null && string.IsNullOrEmpty(constantValueSP.managedReferenceFullTypename))
            {
                constantValueSP.managedReferenceValue = TryCreateDefaultManagedReferenceValue(argumentType);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize();
            CacheDataThisFrame(property);
            EnsureConstantTypeMatching();
            EnsureConstantValueValidity(property);

            float height = 0.0f;

            if (!CanArgumentTypeBeingInstantiated(_argumentType))
            {
                height += TypeSelectorGUI.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }

            if (CanBeDrawn() && !string.IsNullOrEmpty(_constantValueSP.managedReferenceFullTypename))
            {
                height += EditorGUI.GetPropertyHeight(_internValueSP, _internValueSP.isExpanded) + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        private bool CanBeDrawn() => (_argumentType != null && _internValueSP != null);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize();
            CacheDataThisFrame(property);
            EnsureConstantTypeMatching();
            EnsureConstantValueValidity(property);
            
            position.height = EditorGUIUtility.singleLineHeight;

#if USE_INTERFACE_PROPERTY_DRAWER
            if (!CanArgumentTypeBeingInstantiated(_argumentType))
            {
                position = DrawTypeInstantiation(position);
            }
#endif

            if (!CanBeDrawn())
            {
                return;
            }

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

        private Rect DrawTypeInstantiation(Rect position)
        {
            var options = new TypeSelectorAdvancedDropdown.Settings()
            {
                ConstraintType = GetTypeOrArrayElementType(_argumentType),
                UsageFlags = TypeCodebase.ETypeUsageFlag.Class | TypeCodebase.ETypeUsageFlag.ForbidUnityObject
            };

            var constantValue = _constantValueSP.managedReferenceValue as IConstantValue;
            Type constantValueType = (constantValue.Value != null ? constantValue.Value.GetType() : null);
            position = TypeSelectorGUI.Draw(position, constantValueType, options.ConstraintType, options, out bool hasSelectedType, out Type selectedType);
            if (hasSelectedType)
            {
                if (selectedType == null)
                {
                    constantValue.Value = null;
                }
                else
                {
                    constantValue.Value = Activator.CreateInstance(selectedType);
                }
            }

            return position;
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

            var parameterInfoSP = property.GetParent();
            _parentParameterTypeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);

            _shouldDrawOnlyValue = StandardConstantValues.ShouldDrawValueOnly(_argumentType);
            if (_shouldDrawOnlyValue)
            {
                _internValueSP = _constantValueSP.FindPropertyRelative(ConstantInternalValueSPName);
            }
            else
            {
                _internValueSP = _constantValueSP.Copy();
            }
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

        private static object TryCreateDefaultManagedReferenceValue(Type argumentType)
        {
            try
            {
                bool isArray = argumentType.IsArray;
                Type elementType = (isArray ? argumentType.GetElementType() : argumentType);
                
                foreach (var constantValueType in StandardConstantValues.ConstantMapping)
                {
                    if (constantValueType.Key.IsAssignableFrom(elementType))
                    {
                        return CreateInstanceOrEmptyArray(constantValueType.Value, isArray);
                    }
                }

                IConstantValue gObj = new StandardConstantValues.GenericObject();
                gObj.Value = (CanArgumentTypeBeingInstantiated(elementType) ? CreateInstanceOrEmptyArray(elementType, isArray) : null);
                return gObj;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return null;
            }
        }

        private static object CreateInstanceOrEmptyArray(Type type, bool isArray)
        {
            if (isArray)
            {
                return Array.CreateInstance(type, 0);
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }

        private static Type GetTypeOrArrayElementType(Type type)
            => (type.IsArray ? type.GetElementType() : type);

        private static bool CanArgumentTypeBeingInstantiated(Type type)
            => CanTypeBeInstantiated(GetTypeOrArrayElementType(type));

        private static bool CanTypeBeInstantiated(Type type) => !type.IsAbstract && !type.IsInterface;
    }
}