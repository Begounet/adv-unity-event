using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    public static class MethodPreviewBuilder
    {
        private const string Undefined = "<undefined>";
        private const string Error = "<error>";

        public static string GenerateMethodDisplayName(MethodInfo methodInfo,
            bool displayReturnType = true,
            Func<ParameterInfo, int, string> postParameterHook = null)
        {
            ParameterInfo[] pis = methodInfo.GetParameters();

            // If property...
            if (methodInfo.IsSpecialName)
            {
                string properytName = methodInfo.Name.Remove(0, 4);

                // If setter...
                if (methodInfo.ReturnType == typeof(void))
                {
                    string setterName = string.Empty;
                    if (displayReturnType)
                    {
                        setterName += $"{AUEUtils.MakeHumanDisplayType(pis[0].ParameterType)} ";
                    }
                    setterName += properytName;
                    if (postParameterHook != null)
                    {
                        return $"{setterName}{postParameterHook.Invoke(pis[0], 0)}";
                    }
                    return setterName;
                }
                // else is getter...
                else
                {
                    string getterName = string.Empty;
                    if (displayReturnType)
                    {
                        getterName += $"{AUEUtils.MakeHumanDisplayType(methodInfo.ReturnType)} ";
                    }
                    getterName += properytName;
                    return getterName;
                }
            }

            // Otherwise, it is a method...
            StringBuilder paramsSb = new StringBuilder();
            for (int i = 0; i < pis.Length; ++i)
            {
                paramsSb.Append($"{AUEUtils.MakeHumanDisplayType(pis[i].ParameterType)} {pis[i].Name}");
                if (postParameterHook != null)
                {
                    paramsSb.Append(postParameterHook.Invoke(pis[i], i));
                }
                if (i + 1 < pis.Length)
                {
                    paramsSb.Append(", ");
                }
            }

            string methodName = string.Empty;
            if (displayReturnType)
            {
                methodName += $"{AUEUtils.MakeHumanDisplayType(methodInfo.ReturnType)} ";
            }
            methodName += $"{methodInfo.Name}({paramsSb})";
            return methodName;
        }

        public static string GenerateMethodPreview(SerializedProperty aueMethodSP, MethodInfo methodInfo = null, bool displayReturnType = true)
        {
            if (methodInfo == null)
            {
                methodInfo = AUEUtils.GetMethodInfoFromAUEMethod(aueMethodSP);
                if (methodInfo == null)
                {
                    return Undefined;
                }
            }

            var parameterInfosSP = aueMethodSP.FindPropertyRelative(AUEUtils.ParameterInfosSPName);
            return GenerateMethodDisplayName(methodInfo, displayReturnType, (pi, paramIdx) =>
            {
                var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(paramIdx);
                return $": {GenerateParameterPreview(parameterInfoSP)}";
            });
        }

        private static string GenerateParameterPreview(SerializedProperty parameterInfoSP)
        {
            var customArgumentSP = parameterInfoSP.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
            if (string.IsNullOrEmpty(customArgumentSP.managedReferenceFullTypename))
            {
                return Error;
            }

            var modeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ModeSPName);
            var mode = (AUEMethodParameterInfo.EMode)modeSP.intValue;
            switch (mode)
            {
                case AUEMethodParameterInfo.EMode.Dynamic:
                    return GenerateParameterDynamicPreview(customArgumentSP);
                case AUEMethodParameterInfo.EMode.Constant:
                    return GenerateParameterConstantPreview(customArgumentSP);
                case AUEMethodParameterInfo.EMode.Method:
                    return GenerateParameterMethodPreview(customArgumentSP);
                case AUEMethodParameterInfo.EMode.Property:
                    return GenerateParameterPropertyPreview(customArgumentSP);
                default:
                    return Undefined;
            }
        }

        private static string GenerateParameterDynamicPreview(SerializedProperty customArgumentSP)
        {
            var sourceArgumentIndexSP = customArgumentSP.FindPropertyRelative(AUEUtils.CADynamicSourceArgumentIndexSPName);
            int idx = sourceArgumentIndexSP.intValue;
            if (idx < 0)
            {
                return Undefined;
            }
            return $"{{arg{idx}}}";
        }

        private static string GenerateParameterConstantPreview(SerializedProperty customArgumentSP)
        {
            var constantValueSP = customArgumentSP.FindPropertyRelative(AUEUtils.CAConstantValueSPName);
            var constantValue = constantValueSP.GetTarget<IConstantValue>();
            if (constantValue != null && constantValue.Value != null)
            {
                // Special case for strings. We add ' " ' around.
                if (constantValue.Value is string str)
                {
                    return $"\"{str}\"";
                }
                // Specal case for enums
                else if (constantValue is StandardConstantValues.EnumValue)
                {
                    var constantTypeSP = customArgumentSP.FindPropertyRelative(AUEUtils.CAConstantTypeSPName);
                    Type constantType = SerializableTypeHelper.LoadType(constantTypeSP);
                    return EnumDisplayNameHelper.GetName((Enum)Enum.ToObject(constantType, constantValue.Value), constantType);
                }
                return constantValue.Value.ToString();
            }
            return Undefined;
        }

        private static string GenerateParameterMethodPreview(SerializedProperty customArgumentSP)
        {
            var methodIdSP = customArgumentSP.FindPropertyRelative(AUEUtils.MethodIdSPName);
            var aueRoot = AUEUtils.FindAUERootInParent(customArgumentSP);
            var methodDatabaseSP = aueRoot.FindPropertyRelative(AUEUtils.MethodDatabaseSPName);
            var methodSP = AUEMethodDatabaseUtils.FindMethodById(methodDatabaseSP, (byte)methodIdSP.intValue);
            if (methodSP != null)
            {
                return GenerateMethodPreview(methodSP, displayReturnType: false);
            }
            return Undefined;
        }

        private static string GenerateParameterPropertyPreview(SerializedProperty customArgumentSP)
        {
            var propertyPathSP = customArgumentSP.FindPropertyRelative(AUECAPropertyPropertyDrawer.PropertyPathSPName);

            var sourceModeSP = customArgumentSP.FindPropertyRelative(AUECAPropertyPropertyDrawer.SourceModeSPName);
            if ((AUECAProperty.ESourceMode) sourceModeSP.enumValueIndex == AUECAProperty.ESourceMode.Target)
            {
                var targetSP = customArgumentSP.FindPropertyRelative(AUECAPropertyPropertyDrawer.TargetSPName);
                if (targetSP.objectReferenceValue == null)
                {
                    return "<no target>";
                }
                return $"{targetSP.objectReferenceValue.name}.{propertyPathSP.stringValue}";
            }
            else
            {
                var argIdxSP = customArgumentSP.FindPropertyRelative(AUECAPropertyPropertyDrawer.ArgIndexSPName);
                return $"{{arg{argIdxSP.intValue}}}.{propertyPathSP.stringValue}";
            }
        }
    }
}
