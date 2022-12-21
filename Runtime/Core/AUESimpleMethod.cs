using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace AUE
{
    [System.Diagnostics.DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public class AUESimpleMethod : ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        public static bool IsRegisteringMethods = false;
        public static HashSet<MethodInfo> RegisteredMethods = new HashSet<MethodInfo>();
        public static HashSet<MemberInfo> RegisteredMembers = new HashSet<MemberInfo>();
        public static AUESettings.ENullTargetBehavior NullTargetBehavior = AUESettings.ENullTargetBehavior.Exception;

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
        protected BindingFlags _bindingFlags = DefaultBindingFlags.AUESimpleMethod;
        public BindingFlags BindingFlags { get => _bindingFlags; set => _bindingFlags = value; }

        // Set by the property drawer when method is selected
        [SerializeField]
        protected AUEMethodParameterInfo[] _parameterInfos;
        internal AUEMethodParameterInfo[] ParameterInfos => _parameterInfos;

        public bool IsStatic => (_staticType?.IsValidType ?? false);

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public string PrettyName => GeneratePrettyName();        

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
                Debug.LogError($"Exception raised from {nameof(AUESimpleMethod)} ({PrettyName}): {_identifier}");
                Debug.LogException(ex);
                return null;
            }
#endif

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid()
        {
#if UNITY_EDITOR
            // May happen not being on main thread during AOT registering.
            // In that special case, ignore target check because it will not work during serialization step.
            if (!UnityEditorInternal.InternalEditorUtility.CurrentThreadIsMainThread())
            {
                return (_staticType.IsValidType && !string.IsNullOrWhiteSpace(_methodName));
            }
            else
#endif
            {
                return ((_staticType.IsValidType || _target != null) && !string.IsNullOrWhiteSpace(_methodName));
            }
        }

        internal void SetDirty()
        {
            _cache = null;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
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
            return targetType.GetMethod(_methodName, _bindingFlags, binder: null, methodParameterTypes, modifiers: null);
        }

        internal MethodInfo GetSafeMethod()
        {
            Type targetType = GetTargetType();
            if (targetType == null)
            {
                return null;
            }

            Type[] parameterTypes = _parameterInfos.Where((pi) => pi.ParameterType != null).Select((pi) => pi.ParameterType).ToArray();
            return targetType.GetMethod(_methodName, _bindingFlags, binder: null, parameterTypes, modifiers: null);
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

#if UNITY_EDITOR || (DEVELOPMENT_BUILD && AUE_SAFE)
        internal MethodInfo GetSafeVerboseMethod()
        {
            try
            {
                if (!AUERuntimeUtils.IsUnityObjectValid(_target))
                {
#if UNITY_EDITOR
                    if (NullTargetBehavior == AUESettings.ENullTargetBehavior.Exception)
                    {
#endif
                        throw new Exception($"[{_identifier}] Unset target for method call ({_methodName})");
#if UNITY_EDITOR
                    }
                    else // if (NullTargetBehavior == AUESettings.ENullTargetBehavior.Ignore)
                    {
                        return null;
                    }
#endif
                }

                Type targetType = GetTargetType();
                if (targetType == null)
                {
                    return null;
                }

                Type[] parameterTypes = _parameterInfos
                    .Select((pi) => pi.ParameterType).ToArray();

                MethodInfo mi = targetType.GetMethod(_methodName, _bindingFlags, binder: null, parameterTypes, modifiers: null);
                if (mi == null)
                {
                    throw new InvalidOperationException();
                }
                if (_returnType != null && (_returnType.IsValidType && !_returnType.Type.IsAssignableFrom(mi.ReturnType)))
                {
                    throw new Exception($"[{_identifier}] Unexpected method return type {mi.ReturnType.FullName}. Expected {_returnType.Type.FullName}");
                }
                return mi;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                string error = $"[{_identifier}] Could not get method {_methodName} from class {GetTargetType()?.FullName ?? "unknown"} from assembly {GetTargetType()?.Assembly.ToString() ?? "unknown"}";
#if UNITY_EDITOR
                error = $"[{UnityEngine.SceneManagement.SceneManager.GetActiveScene().path}]" + error;
#endif
                Debug.LogError(error);
                return null;
            }
        }
#endif

#if UNITY_EDITOR
        protected virtual void RegisterForAOT()
        {
            MethodInfo mi = GetSafeVerboseMethod();
            if (mi != null)
            {
                RegisteredMethods.Add(mi);
            }
        }
#endif

        private string GeneratePrettyName()
        {
            var sb = UnsafeGenericPool<StringBuilder>.Get();
            {
                sb.Clear();
                sb.Append(_returnType != null && _returnType.IsValidType ? _returnType.Type.Name : "*");
                sb.Append(' ');
                sb.Append('(');
                if (UnityThread.allowsAPI)
                {
                    if (_target != null)
                    {
                        sb.Append(_target.name);
                    }
                    else
                    {
                        sb.Append("<none>");
                    }
                }
                else
                {
                    sb.Append("<not main thread>");
                }
                sb.Append(").");
                sb.Append(!string.IsNullOrWhiteSpace(_methodName) ? _methodName : "<undefined>");
                sb.Append('(');
                if (_parameterInfos != null)
                {
                    for (int i = 0; i < _parameterInfos.Length; ++i)
                    {
                        var pi = _parameterInfos[i];
                        sb.Append(pi.ParameterType.Name);
                        sb.Append(" (");
                        sb.Append(pi.Mode);
                        sb.Append(')');
                        if (i + 1 < _parameterInfos.Length)
                        {
                            sb.Append(", ");
                        }
                    }
                }
                sb.Append(')');
            }
            string result = sb.ToString();
            UnsafeGenericPool<StringBuilder>.Release(sb);
            return result;
        }
    }
}
