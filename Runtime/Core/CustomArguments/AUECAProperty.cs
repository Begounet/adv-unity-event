using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AUE
{
    [Serializable]
    public class AUECAProperty : IAUECustomArgument
    {
        public enum ESourceMode
        {
            Target,
            Argument
        }

        public enum EExecutionSafeMode
        {
            [EnumDescription("Throws exception if any issue.")]
            Unsafe,

            [EnumDescription("Exception is catched and default value (null) is used")]
            Default
        }

        [SerializeField]
        private ESourceMode _sourceMode = ESourceMode.Target;

        [SerializeField]
        private EExecutionSafeMode _executionSafeMode = EExecutionSafeMode.Unsafe;

        [SerializeField]
        private UnityEngine.Object _target;

        [SerializeField]
        private int _argIndex;

        [SerializeField]
        private string _propertyPath;

        private CAPropertyCache _propertyCache = null;

        object IAUECustomArgument.GetArgumentValue(IAUEMethod aueMethod, Type ParameterType, object[] args)
        {
            object src = null;
            switch (_sourceMode)
            {
                case ESourceMode.Target:
                    src = _target;
                    break;
                case ESourceMode.Argument:
                    src = (_argIndex >= 0 && _argIndex < args.Length ? args[_argIndex] : null);
                    break;
            }

            if (src == null)
            {
                return null;
            }

            if (_propertyCache == null)
            {
                BuildCache(src.GetType());
            }

            if (!_propertyCache.IsValid)
            {
                return null;
            }

            return _propertyCache.GetValue(_executionSafeMode, src);
        }

        private void BuildCache(Type targetType)
        {
            _propertyCache = new CAPropertyCache();
            _propertyCache.BuildCache(targetType, _propertyPath);
        }

        public void SetDirty()
        {
            _propertyCache = null;
        }

#if UNITY_EDITOR
        internal void RegisterForAOT(IMethodArgumentsOwner argumentTypesOwner, HashSet<MemberInfo> outPropertyPathMembers)
        {
            Type targetType = null;
            switch (_sourceMode)
            {
                case ESourceMode.Target:
                    if (_target != null)
                    {
                        targetType = _target.GetType();
                    }
                    break;
                case ESourceMode.Argument:
                    if (_argIndex >= 0)
                    {
                        targetType = argumentTypesOwner.GetArgumentTypes()
                            .Skip(_argIndex)
                            .FirstOrDefault();
                    }
                    break;
            }

            if (targetType != null)
            {
                AOTHelper.RegisterMemberInfosInPropertyPath(outPropertyPathMembers, targetType, _propertyPath);
            }
        }
#endif
    }
}
