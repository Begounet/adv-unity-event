using System;
using System.Reflection;
using UnityEngine;

namespace AUE
{
    public class BaseAUEGet
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

        [SerializeField]
        private SerializableType _returnType = new SerializableType();

        protected T Invoke<T>(params object[] args)
        {
            return (T)_method.Invoke(args);
        }

        public void SetReturnType(Type type)
        {
            _returnType.Type = type;
        }

        public void AddArgumentType(Type type)
        {
            _method.ArgumentTypes.Add(new SerializableType(type));
        }
    }
}