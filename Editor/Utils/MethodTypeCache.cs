using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AUE
{
    public static class MethodTypeCache
    {
        [DebuggerDisplay("{value}", Name = "{key}")]
        private static Dictionary<MethodFilter, AUEUtils.MethodMetaData[]> s_entries = new Dictionary<MethodFilter, AUEUtils.MethodMetaData[]>();

        public static void GetMethods(
            MethodFilter methodFilter,
            List<AUEUtils.MethodMetaData> outMethodMetaData)
        {
            outMethodMetaData.AddRange(CreateOrGetMethodMetaData(methodFilter));
        }

        public static AUEUtils.MethodMetaData GetMethod(string methodName, MethodFilter methodFilter, Type[] parameterTypes)
        {
            var methodMetaDataArray = CreateOrGetMethodMetaData(methodFilter);
            for (int i = 0; i < methodMetaDataArray.Length; ++i)
            {
                if (methodMetaDataArray[i].MethodInfo.Name == methodName)
                {
                    ParameterInfo[] pis = methodMetaDataArray[i].MethodInfo.GetParameters();
                    if (AUEUtils.DoesParametersMatchExactly(pis, parameterTypes))
                    {
                        return methodMetaDataArray[i];
                    }
                }
            }
            return null;
        }

        private static AUEUtils.MethodMetaData[] CreateOrGetMethodMetaData(MethodFilter methodFilter)
        {
            if (!s_entries.TryGetValue(methodFilter, out AUEUtils.MethodMetaData[] methodMetaData))
            { 
                AUEUtils.LoadAllMethodsInfo(methodFilter, out methodMetaData);
                s_entries.Add(methodFilter, methodMetaData);
            }
            return methodMetaData;
        }
    }
}