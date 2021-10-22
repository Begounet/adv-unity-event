using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    [Serializable]
    public class AUECAMethodReference : IAUECustomArgument
    {
#if UNITY_EDITOR
        [SerializeField]
        private byte _methodId;
#endif

        [SerializeField]
        private int _methodIndex = -1;

        object IAUECustomArgument.GetArgumentValue(IMethodDatabaseOwner methodDbOwner, Type ParameterType, object[] args)
        {
            if (_methodIndex < 0)
            {
                return null;
            }

            var method = methodDbOwner.MethodDatabase[_methodIndex];
            return method.Invoke(methodDbOwner, args);
        }

        bool IAUECustomArgument.IsValid(IMethodDatabaseOwner methodDbOwner, Type ParameterType)
        {
            return _methodIndex >= 0 &&
                _methodIndex < methodDbOwner.MethodDatabase.Count &&
                methodDbOwner.MethodDatabase[_methodIndex].IsValid();
        }
    }
}
