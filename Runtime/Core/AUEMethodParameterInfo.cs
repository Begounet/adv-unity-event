using System;
using System.Diagnostics;
using UnityEngine;

namespace AUE
{
    [Serializable]
    [DebuggerDisplay("{_parameterType} ({_mode})")]
    internal class AUEMethodParameterInfo
    {
        public enum EMode
        {
            Dynamic,
            Constant,
            Method
        }

#pragma warning disable 0414
        [SerializeField]
        private EMode _mode = EMode.Constant;
#pragma warning restore

        [SerializeField]
        private SerializableType _parameterType;
        public Type ParameterType => _parameterType.Type;

        [SerializeReference]
        private IAUECustomArgument _customArgument;

        internal object GetValue(IMethodDatabaseOwner methodDbOwner, object[] args)
        {
            return (_customArgument?.GetArgumentValue(methodDbOwner, ParameterType, args) ?? null);
        }
    }
}