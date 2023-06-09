using AUE.Descriptors;
using System;
using System.Diagnostics;
using TypeCodebase;
using UnityEngine;

namespace AUE
{
    [Serializable]
    [DebuggerDisplay("{ParameterType} ({_mode})")]
    public class AUEMethodParameterInfo
    {
        public enum EMode
        {
            Dynamic,
            Constant,
            Property,
            Method
        }

#pragma warning disable 0414
        [SerializeField]
        private EMode _mode = EMode.Constant;
        public EMode Mode => _mode;
#pragma warning restore

        [SerializeField]
        private SerializableType _parameterType;
        public Type ParameterType => _parameterType.Type;

        [SerializeReference]
        private IAUECustomArgument _customArgument;
        internal IAUECustomArgument CustomArgument => _customArgument;

        internal object GetValue(IAUEMethod aueMethod, object[] args)
        {
            return (_customArgument?.GetArgumentValue(aueMethod, ParameterType, args) ?? null);
        }

        public AUEMethodParameterInfo() { }

        public AUEMethodParameterInfo(AUEParameterDescriptor desc)
        {
            _mode = desc.Mode;
            _parameterType = new SerializableType(desc.ParameterType);
            _customArgument = desc.CustomArgument;
        }
    }
}