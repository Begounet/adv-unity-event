using System;
using TypeCodebase;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECADynamic))]
    public class AUECADynamicPropertyDrawer : PropertyDrawer
    {
        private const string CastSettingsSPName = "_castSettings";
        private const string AllCastSettingsSettingsSPName = "_settings";

        [System.Serializable]
        private class AllCastSettings : ScriptableObject
        {
            [SerializeReference]
            private ICastSettings[] _settings;
        }

        private AllCastSettings _allCastSettings;
        private SerializedProperty _allCastSettingsSP;

        public static void Initialize(SerializedProperty property)
        {
            var paramInfoSP = property.GetParent();
            var paramInfoTypeSP = paramInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
            var paramInfoType = SerializableTypeHelper.LoadType(paramInfoTypeSP);

            var sourceArgumentIndexSP = property.FindPropertyRelative(AUEUtils.CADynamicSourceArgumentIndexSPName);
            sourceArgumentIndexSP.intValue = -1;

            var castSettingsSP = property.FindPropertyRelative(CastSettingsSPName);
            Type[] argumentTypes = AUEUtils.LoadMethodDynamicParameterTypes(property);
            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                if (DoesMethodParameterMatchArgumentType(paramInfoType, argumentTypes[i]) ||
                    Caster.CanBeCasted(argumentTypes[i], paramInfoType))
                {
                    sourceArgumentIndexSP.intValue = i;
                    TryLoadCastSettings(castSettingsSP, paramInfoType, argumentTypes[i]);
                    break;
                }
            }
        }

        private static void TryLoadCastSettings(SerializedProperty castSettingsSP, Type srcType, Type dstType)
        {
            Type castSettingsType = Caster.GetCastSettingsType(srcType, dstType);
            if (castSettingsType == null)
            {
                castSettingsSP.managedReferenceValue = null;
            }
            else if (string.IsNullOrEmpty(castSettingsSP.managedReferenceFullTypename) || 
                castSettingsSP.managedReferenceFullTypename != castSettingsType.FullName)
            {
                castSettingsSP.managedReferenceValue = (ICastSettings)Activator.CreateInstance(castSettingsType);
            }
        }

        private void InitializeAllCastSettings(SerializedProperty property)
        {
            var aueRootSP = AUEUtils.FindAUERootInParent(property);
            var argumentTypesSP = aueRootSP.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);

            if (_allCastSettings != null && _allCastSettingsSP != null && _allCastSettingsSP.arraySize == argumentTypesSP.arraySize)
            {
                return;
            }

            var paramInfoSP = property.GetParent();
            var paramInfoTypeSP = paramInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
            var paramInfoType = SerializableTypeHelper.LoadType(paramInfoTypeSP);

            _allCastSettings = ScriptableObject.CreateInstance<AllCastSettings>();
            var allCastSettingsSO = new SerializedObject(_allCastSettings);
            _allCastSettingsSP = allCastSettingsSO.FindProperty(AllCastSettingsSettingsSPName);

            Type[] argumentTypes = AUEUtils.LoadMethodDynamicParameterTypes(property);
            _allCastSettingsSP.arraySize = argumentTypes.Length;
            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                var castSettingsSP = _allCastSettingsSP.GetArrayElementAtIndex(i);
                Type castSettingsType = Caster.GetCastSettingsType(argumentTypes[i], paramInfoType);
                if (castSettingsType != null)
                {
                    castSettingsSP.managedReferenceValue = (ICastSettings)Activator.CreateInstance(castSettingsType);
                }
                else
                {
                    castSettingsSP.managedReferenceValue = null;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += GetCasterSettingsHeight(property) + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        private float GetCasterSettingsHeight(SerializedProperty property)
        {
            InitializeAllCastSettings(property);

            var paramInfoSP = property.GetParent();
            var paramInfoTypeSP = paramInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
            var paramInfoType = SerializableTypeHelper.LoadType(paramInfoTypeSP);

            Type[] argumentTypes = AUEUtils.LoadMethodDynamicParameterTypes(property);
            float height = 0.0f;
            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                var castSettingsSP = _allCastSettingsSP.GetArrayElementAtIndex(i);
                height = Mathf.Max(height, GetCastSettingsChildrenPropertiesHeight(castSettingsSP));
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type[] argumentTypes = AUEUtils.LoadMethodDynamicParameterTypes(property);

            position.height = EditorGUIUtility.singleLineHeight;
            if (argumentTypes.Length == 0)
            {
                DrawNoArguments(position);
            }
            else
            {
                DrawArgumentsSelection(position, argumentTypes, property);
            }
        }

        private void DrawNoArguments(Rect position)
        {
            EditorGUI.LabelField(position, "No argument supplied. Cannot use Dynamic mode.");
        }

        private void DrawArgumentsSelection(Rect position, Type[] argumentTypes, SerializedProperty property)
        {
            InitializeAllCastSettings(property);

            Rect argumentRect = position;
            argumentRect.width /= argumentTypes.Length;

            var paramInfoSP = property.GetParent();
            var paramInfoTypeSP = paramInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
            var paramInfoType = SerializableTypeHelper.LoadType(paramInfoTypeSP);

            var sourceArgumentIndexSP = property.FindPropertyRelative(AUEUtils.CADynamicSourceArgumentIndexSPName);
            var castSettingsSP = property.FindPropertyRelative(CastSettingsSPName);

            int oldArgumentIndex = sourceArgumentIndexSP.intValue;
            int newArgumentIndex = oldArgumentIndex;
            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                var argumentType = argumentTypes[i];
                bool doesMethodParameterMatchArgumentType = DoesMethodParameterMatchArgumentType(paramInfoType, argumentType) || Caster.CanBeCasted(argumentType, paramInfoType);
                EditorGUI.BeginDisabledGroup(!doesMethodParameterMatchArgumentType);
                if (EditorGUI.ToggleLeft(argumentRect, AUEUtils.MakeHumanDisplayType(argumentType), doesMethodParameterMatchArgumentType && i == newArgumentIndex))
                {
                    newArgumentIndex = i;
                }
                EditorGUI.EndDisabledGroup();
                argumentRect.x += argumentRect.width;
            }
            if (newArgumentIndex != oldArgumentIndex)
            {
                sourceArgumentIndexSP.intValue = newArgumentIndex;

                Type castSettingsType = Caster.GetCastSettingsType(argumentTypes[newArgumentIndex], paramInfoType);
                castSettingsSP.managedReferenceValue = (castSettingsType != null ? (ICastSettings)Activator.CreateInstance(castSettingsType) : null);
            }

            argumentRect.y += argumentRect.height + EditorGUIUtility.standardVerticalSpacing;
            argumentRect.x = position.x;

            SerializedProperty currentCastSettingsSP;
            for (int i = 0; i < argumentTypes.Length; ++i)
            {
                if (sourceArgumentIndexSP.intValue == i)
                {
                    TryLoadCastSettings(castSettingsSP, argumentTypes[i], paramInfoType);
                    currentCastSettingsSP = castSettingsSP;
                }
                else
                {
                    currentCastSettingsSP = _allCastSettingsSP.GetArrayElementAtIndex(i);
                }

                if (!string.IsNullOrEmpty(currentCastSettingsSP.managedReferenceFullTypename))
                {
                    Rect settingsRect = 
                        new Rect(argumentRect.x, argumentRect.y, 
                        argumentRect.width, 
                        EditorGUI.GetPropertyHeight(currentCastSettingsSP) - EditorGUIUtility.singleLineHeight);

                    DrawCastSettingsProperties(settingsRect, currentCastSettingsSP.Copy());
                }
            }
        }

        private float GetCastSettingsChildrenPropertiesHeight(SerializedProperty castSettingsSP)
        {
            SerializedProperty endSP = castSettingsSP.Copy();
            endSP.NextVisible(false);

            SerializedProperty it = castSettingsSP.Copy();

            float height = 0;
            it.NextVisible(true);
            while (!SerializedProperty.EqualContents(it, endSP))
            {
                height += EditorGUI.GetPropertyHeight(it);
                if (!it.NextVisible(it.isExpanded))
                {
                    break;
                }
            }
            return height;
        }

        private void DrawCastSettingsProperties(Rect position, SerializedProperty castSettingsSP)
        {
            SerializedProperty endSP = castSettingsSP.Copy();
            endSP.NextVisible(false);

            SerializedProperty it = castSettingsSP.Copy();

            it.NextVisible(true);
            while (!SerializedProperty.EqualContents(it, endSP))
            {
                position.height = EditorGUI.GetPropertyHeight(it);
                EditorGUI.PropertyField(position, it, GUIContent.none, includeChildren: it.isExpanded);
                if (!it.NextVisible(it.isExpanded))
                {
                    break;
                }
            }
        }

        private static bool DoesMethodParameterMatchArgumentType(Type methodParamType, Type dynamicType)
            => (dynamicType == methodParamType || dynamicType.IsSubclassOf(methodParamType));
    }
}