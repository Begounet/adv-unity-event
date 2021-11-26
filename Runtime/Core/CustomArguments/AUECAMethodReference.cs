using System;
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

        object IAUECustomArgument.GetArgumentValue(IAUEMethod aueMethod, Type ParameterType, object[] args)
        {
            if (!IsValid(aueMethod))
            {
                return null;
            }

            var method = aueMethod.MethodDatabase[_methodIndex];
            return method.Invoke(aueMethod, args);
        }

        private bool IsValid(IAUEMethod aueMethod) =>
            _methodIndex >= 0
            && _methodIndex < aueMethod.MethodDatabase.Count
            && aueMethod.MethodDatabase[_methodIndex].IsValid();
    }
}
