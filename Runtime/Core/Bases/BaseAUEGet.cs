using System;
using System.Reflection;
using UnityEngine;

namespace AUE
{
    public class BaseAUEGet : ISerializationCallbackReceiver
    {
        [SerializeField]
        private AUEMethod _method = new AUEMethod()
        {
            BindingFlags = BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.GetProperty
                    | BindingFlags.GetField
        };

        protected T Invoke<T>(params object[] args)
        {
            return (T)_method.Invoke(args);
        }

        public void DefineReturnAndParametersType(Type returnType, params Type[] paramTypes)
        {
            MethodSignatureDefinitionHelper.DefineReturnType(_method.ReturnType, returnType);
            MethodSignatureDefinitionHelper.DefineParameterTypes(_method.ArgumentTypes, paramTypes);
        }

        public void OnBeforeSerialize() => OnDefineSignatureMethod();
        public void OnAfterDeserialize() { }

        protected virtual void OnDefineSignatureMethod() { }
    }
}