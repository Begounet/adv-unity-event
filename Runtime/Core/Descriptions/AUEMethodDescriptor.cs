using System;
using System.Reflection;
using UnityEngine.Events;

namespace AUE.Descriptors
{
    public class AUEMethodDescriptor
    {
        public UnityEngine.Object Target { get; set; }
        public string MethodName { get; set; }
        public Type ReturnType { get; set; }
        public Type[] ArgumentTypes { get; set; }
        public AUEParameterDescriptor[] Parameters { get; set; }

        public BindingFlags BindingFlags { get; set; } = DefaultBindingFlags.AUESimpleMethod;

        public UnityEventCallState CallState { get; set; } = UnityEventCallState.RuntimeOnly;

        public AUEMethodDescriptor(UnityEngine.Object target, string methodName, Type returnType, Type[] argumentTypes, AUEParameterDescriptor[] parameters)
        {
            Target = target;
            ArgumentTypes = argumentTypes;
            ReturnType = returnType;
            MethodName = methodName;
            Parameters = parameters;
        }

        public AUEMethodDescriptor(UnityEngine.Object target, string methodName, BindingFlags bindingFlags, AUEParameterDescriptor[] parameters)
            : this(target, target.GetType().GetMethod(methodName, bindingFlags), bindingFlags, parameters)
        { }

        public AUEMethodDescriptor(UnityEngine.Object target, MethodInfo methodInfo, BindingFlags bindingFlags, AUEParameterDescriptor[] parameters)
        {
            Target = target;
            MethodName = methodInfo.Name;
            ReturnType = methodInfo.ReturnType;
            
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            
            ArgumentTypes = new Type[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; ++i)
            {
                ArgumentTypes[i] = parameterInfos[i].ParameterType;
            }

            Parameters = parameters;
        }
    }
}