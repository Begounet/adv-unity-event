using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;
using AUE.Descriptors;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AUE
{
    internal static class UnityEventsUpgrader
    {
        private class ArgumentCacheWrapper
        {
            private FieldInfo _argumentsCacheFI;
            private FieldInfo _boolFI;
            private FieldInfo _intFI;
            private FieldInfo _floatFI;
            private FieldInfo _stringFI;
            private FieldInfo _unityObjectFI;

            private Dictionary<PersistentListenerMode, FieldInfo> _getMapping;

            public ArgumentCacheWrapper(FieldInfo argumentsCacheFI)
            {
                var bf = DefaultBindingFlags.PrivateFields;
                _argumentsCacheFI = argumentsCacheFI;
                var argumentsCacheType = argumentsCacheFI.FieldType;
                _boolFI = argumentsCacheType.GetField("m_BoolArgument", bf);
                _intFI = argumentsCacheType.GetField("m_IntArgument", bf);
                _floatFI = argumentsCacheType.GetField("m_FloatArgument", bf);
                _stringFI = argumentsCacheType.GetField("m_StringArgument", bf);
                _unityObjectFI = argumentsCacheType.GetField("m_ObjectArgument", bf);

                _getMapping = new Dictionary<PersistentListenerMode, FieldInfo>()
                {
                    { PersistentListenerMode.Bool, _boolFI },
                    { PersistentListenerMode.Int, _intFI },
                    { PersistentListenerMode.Float, _floatFI },
                    { PersistentListenerMode.String, _stringFI },
                    { PersistentListenerMode.Object, _unityObjectFI },
                };
            }

            public object GetValue(object call, PersistentListenerMode mode)
            {
                var argumentCache = _argumentsCacheFI.GetValue(call);
                if (_getMapping.TryGetValue(mode, out FieldInfo fi))
                {
                    return fi.GetValue(argumentCache);
                }
                return null;
            }
        }

        public static void ToAUEEvent(UnityEngine.Object owner, UnityEventBase uEvent, BaseAUEEvent aueEvent)
        {
#if UNITY_EDITOR
            aueEvent.ClearEvents();

            if (uEvent == null)
            {
                return;
            }

            var privateFieldBF = DefaultBindingFlags.PrivateFields;
            Type eventType = uEvent.GetType();

            FieldInfo persistentCallsFI = eventType.GetFieldInfoInHierarchy("m_PersistentCalls");
            object persistentCalls = persistentCallsFI.GetValue(uEvent);

            Type persistentCallsType = persistentCallsFI.FieldType;
            FieldInfo callsFI = persistentCallsType.GetField("m_Calls", privateFieldBF);
            IList calls = (IList)callsFI.GetValue(persistentCalls);
            if (calls.Count == 0)
            {
                return;
            }

            Type persistentCallType = calls[0].GetType();
            FieldInfo targetFI = persistentCallType.GetField("m_Target", privateFieldBF);
            FieldInfo methodNameFI = persistentCallType.GetField("m_MethodName", privateFieldBF);
            FieldInfo modeFI = persistentCallType.GetField("m_Mode", privateFieldBF);
            FieldInfo callStateFI = persistentCallType.GetField("m_CallState", privateFieldBF);
            FieldInfo argumentsCacheFI = persistentCallType.GetField("m_Arguments", privateFieldBF);

            var argumentTypes = aueEvent.ArgumentTypes.ToArray();
            var argumentCacheWrapper = new ArgumentCacheWrapper(argumentsCacheFI);

            List<AUEParameterDescriptor> parameterDescs = new List<AUEParameterDescriptor>();
            for (int callIdx = 0; callIdx < calls.Count; ++callIdx)
            {
                var call = calls[callIdx];
                var target = (UnityEngine.Object)targetFI.GetValue(call);
                var paramMode = (PersistentListenerMode)modeFI.GetValue(call);
                var methodName = (string)methodNameFI.GetValue(call);
                var callState = (UnityEventCallState)callStateFI.GetValue(call);

                if (target == null)
                {
                    continue;
                }

                var targetType = target.GetType();
                MethodInfo mi = FindBestMatchingMethod(targetType, methodName, paramMode, argumentTypes);
                if (mi == null)
                {
                    continue;
                }

                ParameterInfo[] pis = mi.GetParameters();

                parameterDescs.Clear();
                if (pis.Length > 0)
                {
                    for (int paramIndex = 0; paramIndex < pis.Length; ++paramIndex)
                    {
                        AUEParameterDescriptor paramDesc = GenerateParameterDescriptor(paramIndex, argumentCacheWrapper, call, paramMode, pis[paramIndex].ParameterType);
                        if (paramDesc != null)
                        {
                            parameterDescs.Add(paramDesc);
                        }
                    }
                }
                var methodDesc = new AUEMethodDescriptor(target, methodName, null, argumentTypes, parameterDescs.ToArray())
                {
                    CallState = callState
                };
                aueEvent.AddEvent(new AUEMethod(methodDesc));
            }

            EditorUtility.SetDirty(owner);
#endif
        }

        private static AUEParameterDescriptor GenerateParameterDescriptor(
            int paramIndex,
            ArgumentCacheWrapper argumentCacheWrapper,
            object call,
            PersistentListenerMode paramMode,
            Type paramType)
        {
            AUEParameterDescriptor paramDesc;
            if (paramMode == PersistentListenerMode.EventDefined)
            {
                // Dynamic
                paramDesc = new AUEParameterDescriptor(AUEMethodParameterInfo.EMode.Dynamic, paramType, new AUECADynamic(sourceArgumentIndex: paramIndex));
            }
            else if (paramMode != PersistentListenerMode.Void)
            {
                if (paramType != null)
                {
                    // Constant
                    paramDesc = new AUEParameterDescriptor(AUEMethodParameterInfo.EMode.Constant, paramType, new AUECAConstant(argumentCacheWrapper.GetValue(call, paramMode)));
                }
                else
                {
                    paramDesc = null;
                }
            }
            else
            {
                paramDesc = null;
            }

            return paramDesc;
        }

        private static MethodInfo FindBestMatchingMethod(Type type, string methodName, PersistentListenerMode mode, Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            var matchingMethods = new List<MethodInfo>();
            for (int i = 0; i < methods.Length; ++i)
            {
                if (methods[i].Name == methodName)
                {
                    matchingMethods.Add(methods[i]);
                }
            }

            if (matchingMethods.Count == 0)
            {
                return null;
            }
            else if (matchingMethods.Count == 1)
            {
                return matchingMethods[0];
            }
            else
            {
                Type expectedParameter = GetParameterTypeFromMode(mode);
                return matchingMethods.FirstOrDefault((mi) =>
                {
                    ParameterInfo[] pis = mi.GetParameters();
                    if (mode == PersistentListenerMode.EventDefined)
                    {
                        return DoesAllParametersMatch(pis, parameterTypes);
                    }

                    bool hasParameters = (pis.Length > 0);
                    if (expectedParameter == null)
                    {
                        return !hasParameters;
                    }
                    else if (expectedParameter != null && !hasParameters)
                    {
                        return false;
                    }

                    return expectedParameter.IsAssignableFrom(pis[0].ParameterType);
                });
            }
        }

        private static bool DoesAllParametersMatch(ParameterInfo[] pis, Type[] parameterTypes)
        {
            if (pis.Length != parameterTypes.Length)
            {
                return false;
            }

            for (int i = 0; i < pis.Length; ++i)
            {
                if (!parameterTypes[i].IsAssignableFrom(pis[i].ParameterType))
                {
                    return false;
                }
            }
            return true;
        }

        private static Type GetParameterTypeFromMode(PersistentListenerMode mode)
        {
            switch (mode)
            {
                case PersistentListenerMode.Object:
                    return typeof(UnityEngine.Object);
                case PersistentListenerMode.Int:
                    return typeof(int);
                case PersistentListenerMode.Float:
                    return typeof(float);
                case PersistentListenerMode.String:
                    return typeof(string);
                case PersistentListenerMode.Bool:
                    return typeof(bool);

                default:
                    return null;
            }
        }

        private static FieldInfo GetFieldInfoInHierarchy(this Type src, string name)
        {
            var privateFieldBF = DefaultBindingFlags.PrivateFields;
            FieldInfo fi = src.GetField(name, privateFieldBF);
            while (src != null && fi == null)
            {
                src = src.BaseType;
                fi = src?.GetField(name, privateFieldBF);
            }
            return fi;
        }
    }
}