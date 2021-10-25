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

        public BindingFlags BindingFlags { get; set; } =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.GetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty
            | BindingFlags.SetField;

        public UnityEventCallState CallState { get; set; } = UnityEventCallState.RuntimeOnly;

        public AUEMethodDescriptor(UnityEngine.Object target, string methodName, Type returnType, Type[] argumentTypes, AUEParameterDescriptor[] parameters)
        {
            Target = target;
            ArgumentTypes = argumentTypes;
            ReturnType = returnType;
            MethodName = methodName;
            Parameters = parameters;
        }
    }
}