using System;
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

        [SerializeField]
        private ESourceMode _sourceMode;

        [SerializeField]
        private UnityEngine.Object _target;

        [SerializeField]
        private int _argIndex;

        [SerializeField]
        private string _propertyPath;

        private CAPropertyCache _propertyCache = null;

        object IAUECustomArgument.GetArgumentValue(IMethodDatabaseOwner methodDbOwner, Type ParameterType, object[] args)
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

            return _propertyCache.GetValue(src);
        }

        private void BuildCache(Type targetType)
        {
            _propertyCache = new CAPropertyCache();
            _propertyCache.BuildCache(targetType, _propertyPath);
        }

        bool IAUECustomArgument.IsValid(IMethodDatabaseOwner methodDbOwner, Type ParameterType) => true;

        public void SetDirty()
        {
            _propertyCache = null;
        }
    }
}
