using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        [SerializeField]
        private EMode _mode = EMode.Constant;

        [SerializeField]
        private SerializableType _parameterType;
        public Type ParameterType => _parameterType.Type;

        [SerializeReference]
        private IAUECustomArgument _customArgument;

        internal object GetValue(IMethodDatabaseOwner methodDbOwner, object[] args)
        {
            return _customArgument.GetArgumentValue(methodDbOwner, ParameterType, args);
        }
    }
}