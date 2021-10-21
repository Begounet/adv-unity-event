using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    public static class AUEUtils
    {
        public const string MethodNameSPName = "_methodName";
        public const string TargetSPName = "_target";
        public const string CallStateSPName = "_callState";
        public const string ReturnTypeSPName = "_returnType";
        public const string MethodDatabaseSPName = "_methodDatabase";
        public const string ParameterInfosSPName = "_parameterInfos";
        public const string BindingFlagsSPName = "_bindingFlags";
        public const string ParameterInfoTypeSPName = "_parameterType";
        public const string CAConstantTypeSPName = "_type";
        public const string CAMethodIdSPName = "_methodId";
        public const string ArgumentTypesSPName = "_argumentTypes";
        public const string CADynamicSourceArgumentIndexSPName = "_sourceArgumentIndex";
        public const string IdSPName = "_id";
        public const string MethodIdSPName = "_methodId";
        public const string MethodIndexSPName = "_methodIndex";
        public const string ModeSPName = "_mode";
        public const string CustomArgumentSPName = "_customArgument";

        private static readonly Dictionary<Type, string> HumanReadableType = new Dictionary<Type, string>()
        {
            { typeof(string), "string" },
            { typeof(int), "int" },
            { typeof(bool), "bool" },
            { typeof(float), "float" },
            { typeof(void), "void" },
            { typeof(object), "object" },
            { typeof(UnityEngine.Object), "UnityEngine.Object" }
        };

        [DebuggerDisplay("{DisplayName}")]
        public class MethodMetaData
        {
            public MethodInfo MethodInfo { get; set; }
            public string DisplayName { get; set; }
        }

        public static bool LoadMethodInfoFromAUEMethod(SerializedProperty aueMethodSP,
            out TargetInvokeInfo[] invokeInfos,
            out InvokeInfo selectedInvoke)
        {
            List<TargetInvokeInfo> invokeInfoList = new List<TargetInvokeInfo>();
            if (!LoadAllInvokeInfosFromAUEMethod(aueMethodSP, invokeInfoList))
            {
                invokeInfos = null;
                selectedInvoke = null;
                return false;
            }

            invokeInfos = invokeInfoList.ToArray();
            selectedInvoke = GetCurentInvokeInfo(aueMethodSP, invokeInfoList);
            return true;
        }

        private static InvokeInfo GetCurentInvokeInfo(SerializedProperty aueMethodSP, List<TargetInvokeInfo> invokeInfos)
        {
            var methodNameSP = aueMethodSP.FindPropertyRelative(MethodNameSPName);
            for (int i = 0; i < invokeInfos.Count; ++i)
            {
                var methodInfos = invokeInfos[i].Methods;
                for (int mIdx = 0; mIdx < methodInfos.Count; ++mIdx)
                {
                    var methodInfo = methodInfos[mIdx];
                    if (methodInfo.MethodInfo.Name == methodNameSP.stringValue && DoesParametersMatch(methodInfo.MethodInfo, aueMethodSP))
                    {
                        return new InvokeInfo()
                        {
                            Target = invokeInfos[i].Target,
                            MethodMeta = methodInfo
                        };
                    }
                }
            }
            return null;
        }

        public static bool LoadAllInvokeInfosFromAUEMethod(SerializedProperty aueMethodSP, List<TargetInvokeInfo> outInvokeInfos)
        {
            var targetSP = aueMethodSP.FindPropertyRelative(TargetSPName);
            if (targetSP.objectReferenceValue == null)
            {
                return false;
            }

            var target = targetSP.objectReferenceValue;
            if (target is Component component)
            {
                target = component.gameObject;
            }

            var bindingFlags = (BindingFlags) aueMethodSP.FindPropertyRelative(BindingFlagsSPName).intValue;

            Type returnType = SerializableTypeHelper.LoadType(aueMethodSP.FindPropertyRelative(ReturnTypeSPName));
            MethodFilter methodFilter = new MethodFilter()
            {
                TargetType = target.GetType(),
                ReturnType = returnType,
                BindingFlags = bindingFlags
            };

            TargetInvokeInfo invokeInfo = new TargetInvokeInfo() { Target = target };
            MethodTypeCache.GetMethods(methodFilter, invokeInfo.Methods);
            outInvokeInfos.Add(invokeInfo);
            if (target is GameObject gameObject)
            {
                Component[] components = gameObject.GetComponents<Component>();
                for (int i = 0; i < components.Length; ++i)
                {
                    invokeInfo = new TargetInvokeInfo() { Target = components[i] };
                    methodFilter = new MethodFilter()
                    {
                        TargetType = components[i].GetType(),
                        ReturnType = returnType,
                        BindingFlags = bindingFlags
                    };
                    MethodTypeCache.GetMethods(methodFilter, invokeInfo.Methods);
                    outInvokeInfos.Add(invokeInfo);
                }
            }
            return true;
        }

        public static void LoadAllMethodsInfo(MethodFilter methodFilter,
            out MethodMetaData[] methodData)
        {
            Type targetType = methodFilter.TargetType;
            MethodInfo[] methods = targetType.GetMethods(methodFilter.BindingFlags);

            methodData = methods
                .Where((mi) =>
                {
                    bool passReturnTypeFilter = (methodFilter.ReturnType == null || methodFilter.ReturnType.IsAssignableFrom(mi.ReturnType));
                    bool passGetPropertyFilter = (methodFilter.BindingFlags.HasFlag(BindingFlags.GetProperty) || !IsGetter(mi));
                    bool passSetPropertyFilter = (methodFilter.BindingFlags.HasFlag(BindingFlags.SetProperty) || !IsSetter(mi));
                    return passReturnTypeFilter && passGetPropertyFilter && passSetPropertyFilter;
                })
                .OrderByDescending((mi) => mi.DeclaringType.Name)
                .ThenBy((mi) => mi.Name)
                .Select((mi) => new MethodMetaData()
                {
                    MethodInfo = mi,
                    DisplayName = GenerateMethodDisplayName(mi)
                })
                .ToArray(); 
        }



        public static ParameterInfo[] LoadParameterTypesFromAUEMethod(SerializedProperty aueMethodSP)
        {
            var mmd = LoadMethodMetaDataFromAUEMethod(aueMethodSP);
            return mmd?.MethodInfo.GetParameters();
        }

        public static MethodMetaData LoadMethodMetaDataFromAUEMethod(SerializedProperty aueMethodSP)
        {
            var targetSP = aueMethodSP.FindPropertyRelative(TargetSPName);
            var methodSP = aueMethodSP.FindPropertyRelative(MethodNameSPName);
            var returnTypeSP = aueMethodSP.FindPropertyRelative(ReturnTypeSPName);
            var parametersTypeSP = aueMethodSP.FindPropertyRelative(ParameterInfosSPName);
            var bindingFlagsSP = aueMethodSP.FindPropertyRelative(BindingFlagsSPName);

            var target = targetSP.objectReferenceValue;
            if (target == null)
            {
                return null;
            }

            var methodName = methodSP.stringValue;
            var returnType = SerializableTypeHelper.LoadType(returnTypeSP);
            var bindingFlags = (BindingFlags) bindingFlagsSP.intValue;

            var methodFilter = new MethodFilter() { TargetType = target.GetType(), ReturnType = returnType, BindingFlags = bindingFlags };
            return MethodTypeCache.GetMethod(methodFilter, LoadTypesFromParameterInfos(parametersTypeSP));
        }

        public static Type[] LoadTypesFromParameterInfos(SerializedProperty aueParameterInfos)
        {
            Type[] paramTypes = new Type[aueParameterInfos.arraySize];
            for (int i = 0; i < aueParameterInfos.arraySize; ++i)
            {
                var parameterInfoSP = aueParameterInfos.GetArrayElementAtIndex(i);
                paramTypes[i] = SerializableTypeHelper.LoadType(parameterInfoSP.FindPropertyRelative(ParameterInfoTypeSPName));
            }
            return paramTypes;
        }

        public static string GenerateMethodDisplayName(MethodInfo methodInfo)
        {
            StringBuilder paramsSb = new StringBuilder();
            Type targetType = methodInfo.DeclaringType;
            ParameterInfo[] pis = methodInfo.GetParameters();
            for (int i = 0; i < pis.Length; ++i)
            {
                paramsSb.Append($"{MakeHumanDisplayType(pis[i].ParameterType)} {pis[i].Name}");
                if (i + 1 < pis.Length)
                {
                    paramsSb.Append(", ");
                }
            }
            // If property...
            if (methodInfo.IsSpecialName)
            {
                string properytName = methodInfo.Name.Remove(0, 4);

                // If setter...
                if (methodInfo.ReturnType == typeof(void))
                {
                    return $"{properytName} ({paramsSb})";
                }
                // else is getter...
                else
                {
                    return $"{MakeHumanDisplayType(methodInfo.ReturnType)} {properytName}";
                }
            }
            return $"{MakeHumanDisplayType(methodInfo.ReturnType)} {methodInfo.Name}({paramsSb})";
        }

        private static bool DoesParametersMatch(MethodInfo methodInfo, SerializedProperty aueMethodSP)
        {
            ParameterInfo[] pis = methodInfo.GetParameters();
            var parameterInfosSP = aueMethodSP.FindPropertyRelative(ParameterInfosSPName);
            if (pis.Length != parameterInfosSP.arraySize)
            {
                return false;
            }

            for (int i = 0; i < pis.Length; ++i)
            {
                var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(i);
                string typeFullQualifiedName = SerializableTypeHelper.GetTypeName(parameterInfoSP.FindPropertyRelative(ParameterInfoTypeSPName));
                if (SerializableTypeHelper.GetTypeName(pis[i].ParameterType) != typeFullQualifiedName)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool DoesParametersMatchExactly(ParameterInfo[] pis, Type[] types)
        {
            if (pis.Length != types.Length)
            {
                return false;
            }

            for (int i = 0; i < pis.Length; ++i)
            {
                if (pis[i].ParameterType != types[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static SerializedProperty FindAUERootInParent(SerializedProperty property)
        {
            // Only the AUE root get a _methodDatabase

            SerializedProperty parent = property;
            while (parent != null)
            {
                var methodDataSP = parent.FindPropertyRelative(MethodDatabaseSPName);
                if (methodDataSP != null)
                {
                    return parent;
                }
                parent = parent.GetParent();
            }
            return null;
        }

        public static string MakeHumanDisplayType(Type t)
        {
            if (HumanReadableType.TryGetValue(t, out string value))
            {
                return value;
            }
            return t.Name;
        }

        public static bool IsMethod(MethodInfo mi) => (!mi.IsSpecialName);
        public static bool IsGetter(MethodInfo mi) => (mi.IsSpecialName && mi.ReturnType != typeof(void));
        public static bool IsSetter(MethodInfo mi) => (mi.IsSpecialName && mi.ReturnType == typeof(void));
    }
}