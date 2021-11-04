using AUE.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace AUE
{
    [Serializable]
    public class AUEMethod : AUESimpleMethod, IMethodDatabaseOwner
    {
        [SerializeField]
        protected UnityEventCallState _callState = UnityEventCallState.RuntimeOnly;
        public UnityEventCallState CallState { get => _callState; set => _callState = value; }

        [SerializeField]
        private List<SerializableType> _argumentTypes = new List<SerializableType>();
        public List<SerializableType> ArgumentTypes
        {
            get => _argumentTypes;
            set => _argumentTypes = value;
        }

        [SerializeField]
        private AUESimpleMethod[] _methodDatabase;
        IList<AUESimpleMethod> IMethodDatabaseOwner.MethodDatabase => _methodDatabase;

        internal object Invoke(params object[] args)
        {
            if (!CanBeExecuted())
            {
                return null;
            }
            return base.Invoke(this, args);
        }

        public AUEMethod() { }

        public AUEMethod(AUEMethodDescriptor desc)
        {
            _target = desc.Target;
            _methodName = desc.MethodName;
            _callState = desc.CallState;
            _returnType = new SerializableType(desc.ReturnType);
            _bindingFlags = desc.BindingFlags;
            _argumentTypes = new List<SerializableType>(desc.ArgumentTypes.Select((argType) => new SerializableType(argType)));
            Type targetType = _target.GetType();
            Type[] paramTypes = GetParameterTypes(desc.Parameters);
            MethodInfo methodInfo = targetType.GetMethod(_methodName, paramTypes);
            ParameterInfo[] parameters = methodInfo.GetParameters();

            Assert.AreEqual(parameters.Length, desc.Parameters.Length, 
                $"It should be as much parameter infos descriptions as parameters of methods ({desc.Parameters.Length} / {parameters.Length})");

            _parameterInfos = new AUEMethodParameterInfo[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                _parameterInfos[i] = new AUEMethodParameterInfo(desc.Parameters[i], parameters[i].ParameterType);
            }
        }

        private bool CanBeExecuted()
        {
            if (_callState == UnityEventCallState.Off)
            {
                return false;
            }
            else if (_callState == UnityEventCallState.RuntimeOnly)
            {
                return UnityEngine.Application.isPlaying;
            }

            return true;
        }

        private static Type[] GetParameterTypes(AUEParameterDescriptor[] parameters)
        {
            var paramTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                paramTypes[i] = parameters[i].ParameterType;
            }
            return paramTypes;
        }
    }
}