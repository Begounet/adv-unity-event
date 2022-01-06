using System;
using System.Reflection;

namespace AUE
{
    [Serializable]
    internal class AUEMethodExecutionCache : IMethodExecutionCache
    {
        private readonly AUESimpleMethod _aueMethod;

        private UnityEngine.Object _cachedTarget;
        private MethodInfo _cachedMethodInfo;
        private object[] _cachedParameters;

        public AUEMethodExecutionCache(AUESimpleMethod aueMethod, bool safeAccess = false)
        {
            _aueMethod = aueMethod;
            _cachedParameters = new object[_aueMethod.ParameterInfos.Length];

            _cachedMethodInfo =
#if AUE_SAFE && DEVELOPMENT_BUILD
                _aueMethod.GetSafeVerboseMethod();
#elif AUE_SAFE
                _aueMethod.GetSafeMethod();
#else
                (safeAccess ? _aueMethod.GetSafeMethod() : _aueMethod.GetFastMethod());
#endif
            _cachedTarget = _aueMethod.IsStatic ? null : _aueMethod.Target;
        }

        object IMethodExecutionCache.Invoke(IAUEMethod aueMethod, params object[] args)
        {
            for (int i = 0; i < _cachedParameters.Length; ++i)
            {
                _cachedParameters[i] = _aueMethod.ParameterInfos[i].GetValue(aueMethod, args);
            }
            return _cachedMethodInfo.Invoke(_cachedTarget, _cachedParameters);
        }        
    }
}