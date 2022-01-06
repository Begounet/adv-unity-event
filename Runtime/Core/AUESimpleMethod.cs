using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
#if DEVELOPMENT_BUILD && AUE_SAFE
using UnityEngine.Assertions;
#endif

namespace AUE
{
    [Serializable]
    public class AUESimpleMethod : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public static bool IsRegisteringMethods = false;
        public static HashSet<MethodInfo> RegisteredMethods = new HashSet<MethodInfo>();
        public static HashSet<MemberInfo> RegisteredMembers = new HashSet<MemberInfo>();

        [SerializeField]
        private byte _id;
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Useful to find a method from code breakpoint to Unity scene.
        /// Use a string property finder to match the identifier.
        /// </summary>
        [SerializeField]
        private string _identifier;
#endif

        /// <summary>
        /// Contains the type when the method is static
        /// </summary>
        [SerializeField]
        private SerializableType _staticType = new SerializableType();

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

        public bool IsStatic => (_staticType?.IsValidType ?? false);

        IMethodExecutionCache _cache;

        internal object Invoke(IAUEMethod aueMethod, params object[] args)
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

            return _cache.Invoke(aueMethod, args);

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
            => ((_staticType.IsValidType || _target != null) && !string.IsNullOrWhiteSpace(_methodName));

        internal void SetDirty()
        {
            _cache = null;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            Cache(safeAccess: true);
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_identifier))
            {
                _identifier = Guid.NewGuid().ToString();
            }

            if (IsRegisteringMethods)
            {
                RegisterForAOT();
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
            Type targetType = GetTargetType();
            Type[] methodParameterTypes = GenerateMethodParameterTypes();
            return targetType.GetMethod(_methodName, methodParameterTypes);
        }

        internal MethodInfo GetSafeMethod()
        {
            Type targetType = GetTargetType();
            if (targetType == null)
            {
                return null;
            }

            Type[] parameterTypes = _parameterInfos.Where((pi) => pi.ParameterType != null).Select((pi) => pi.ParameterType).ToArray();
            return targetType.GetMethod(_methodName, parameterTypes);
        }

        private Type GetTargetType()
        {
#if AUE_SAFE
            try
            {
                if (!IsValid())
                {
                    return null;
                }
#endif
                return (IsStatic ? _staticType.Type : _target.GetType());
#if AUE_SAFE
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
#endif
        }

#if DEVELOPMENT_BUILD && AUE_SAFE
        internal MethodInfo GetSafeVerboseMethod()
        {
            try
            {
                Type targetType = GetTargetType();
                if (targetType == null)
                {
                    return null;
                }

                Type[] parameterTypes = _parameterInfos
                    .Select((pi) => pi.ParameterType).ToArray();

                MethodInfo mi = targetType.GetMethod(_methodName, parameterTypes);
                if (_returnType != null && (_returnType.IsValidType && !_returnType.Type.IsAssignableFrom(mi.ReturnType)))
                {
                    throw new Exception($"Unexpected method return type {mi.ReturnType.FullName}. Expected {_returnType.Type.FullName}");
                }
                return mi;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{_identifier}] Could not get method {_methodName} from class {GetTargetType()?.FullName ?? "unknown"} from assembly {GetTargetType()?.Assembly.ToString() ?? "unknown"}");
                Debug.LogException(ex);
                return null;                    
            }
        }
#endif

#if UNITY_EDITOR
        protected virtual void RegisterForAOT()
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
