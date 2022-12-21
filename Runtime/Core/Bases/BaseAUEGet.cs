using System;
using System.Reflection;
using TypeCodebase;
using UnityEngine;

namespace AUE
{
    public class BaseAUEGet : ISerializationCallbackReceiver
    {
        public bool IsBound => _method.IsValid();

        [SerializeField]
        private AUEMethod _method = new AUEMethod()
        {
            BindingFlags = DefaultBindingFlags.AUEGet
        };

        protected T Invoke<T>(params object[] args)
        {
            return (T)_method.Invoke(args);
        }

        public void DefineReturnAndParametersType(Type returnType, params Type[] paramTypes)
        {
            // Can sometimes happens because... Unity?
            if (_method.ReturnType == null)
            {
                _method.ReturnType = new SerializableType();
            }

            MethodSignatureDefinitionHelper.DefineReturnType(_method.ReturnType, returnType);
            MethodSignatureDefinitionHelper.DefineParameterTypes(_method.ArgumentTypes, paramTypes);
        }

        public void OnBeforeSerialize() => OnDefineSignatureMethod();
        public void OnAfterDeserialize() { }

        protected virtual void OnDefineSignatureMethod() { }
    }
}