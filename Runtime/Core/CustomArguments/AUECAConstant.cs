using System;
using UnityEngine;

namespace AUE
{
    [Serializable]
    public class AUECAConstant : IAUECustomArgument
    {
        [SerializeField]
        private SerializableType _type;

        [SerializeReference]
        private IConstantValue _constantValue;

        object IAUECustomArgument.GetArgumentValue(IMethodDatabaseOwner methodDbOwner, Type ParameterType, object[] args) => _constantValue.GetValue();
        bool IAUECustomArgument.IsValid(IMethodDatabaseOwner methodDbOwner, Type ParameterType) => true;
    }
}
