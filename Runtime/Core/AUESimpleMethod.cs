using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AUE
{
    [Serializable]
    public class AUESimpleMethod : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public static bool IsRegisteringMethods = false;
        public static HashSet<MethodInfo> RegisteredMethods = new HashSet<MethodInfo>();

        [SerializeField]
        private byte _id;
#endif

        /// <summary>
        /// Contains the type when the method is static
        /// </summary>
        [SerializeField]
        private SerializableType _staticType;

        [SerializeField]
        protected UnityEngine.Object _target;
        internal UnityEngine.Object Target => _target;

        [SerializeField]
        protected string _methodName;
        internal string MethodName => _methodName;

        [SerializeField]
        protected SerializableType _returnType = null;
        public SerializableType ReturnType
        {
            get => _returnType;
            set => _returnType = value;
        }

        [SerializeField]
        protected BindingFlags _bindingFlags = 
            BindingFlags.Public 
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.GetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty
            | BindingFlags.SetField;
        public BindingFlags BindingFlags { get => _bindingFlags; set => _bindingFlags = value; }

        // Set by the property drawer when method is selected
        [SerializeField]
        protected AUEMethodParameterInfo[] _parameterInfos;
        internal AUEMethodParameterInfo[] ParameterInfos => _parameterInfos;

        public bool IsStatic => _staticType.IsValidType;

        IMethodExecutionCache _cache;

        internal object Invoke(IMethodDatabaseOwner methodDbOwner, params object[] args)
        {
            if (!IsValid())
            {
                return null;
            }

#if AUE_SAFE
            try
            {
#endif

            if (_cache == null)
            {
                Cache();
            }

            return _cache.Invoke(methodDbOwner, args);

#if AUE_SAFE
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
#endif

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid()
            => (_target != null && !string.IsNullOrWhiteSpace(_methodName));

        internal void SetDirty()
        {
            _cache = null;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            Cache(safeAccess: true);
#if UNITY_EDITOR
            if (IsRegisteringMethods)
            {
                RegisterMethodForAOT();
            }
#endif
        }

        private void Cache(bool safeAccess = false)
        {
            _cache = new AUEMethodExecutionCache(this, safeAccess);
        }

        private Type[] GenerateMethodParameterTypes()
        {
            Type[] types = new Type[_parameterInfos.Length];
            for (int i = 0; i < _parameterInfos.Length; ++i)
            {
                types[i] = _parameterInfos[i].ParameterType;
            }
            return types;
        }

        internal MethodInfo GetFastMethod()
        {
            Type targetType = GetTargeType();
            Type[] methodParameterTypes = GenerateMethodParameterTypes();
            return targetType.GetMethod(_methodName, methodParameterTypes);
        }

        internal MethodInfo GetSafeMethod()
        {
            Type targetType = GetTargeType();
            if (targetType == null)
            {
                return null;
            }

            Type[] parameterTypes = _parameterInfos.Where((pi) => pi.ParameterType != null).Select((pi) => pi.ParameterType).ToArray();
            return targetType.GetMethod(_methodName, parameterTypes);
        }

        private Type GetTargeType()
        {
            return (IsStatic ? _staticType.Type : _target.GetType());
        }

#if UNITY_DEVELOPMENT_BUILD && AUE_SAFE
        internal MethodInfo GetSafeVerboseMethod()
        {
            try
            {
                Type targetType = GetTargeType();
                if (targetType == null)
                {
                    return null;
                }

                Type[] parameterTypes = _parameterInfos.Select((pi) => pi.Type).ToArray();
                MethodInfo mi = targetType.GetMethod(_methodName, parameterTypes);
                if (_returnType.IsValidType && mi.ReturnType != _returnType.Type)
                {
                    throw new Exception($"Unexpected method return type {mi.ReturnType.FullName}. Expected {_returnType.Type.FullName}");
                }
                return mi;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Could not get method {_methodName} from class {GetTargeType()?.FullName ?? "unknown"} from assembly {GetTargeType()?.Assembly.ToString() ?? "unknown"}");
                Debug.LogException(ex);
                return null;                    
            }
        }
#endif

#if UNITY_EDITOR
        private void RegisterMethodForAOT()
        {
            MethodInfo mi = GetSafeMethod();
            if (mi != null)
            {
                RegisteredMethods.Add(mi);
            }
        }
#endif
    }
}
