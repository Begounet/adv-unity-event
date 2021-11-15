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

        public AUECAConstant() { }
        public AUECAConstant(object value)
        {
            Type valueType = value.GetType();

            _type = new SerializableType(valueType);
            _constantValue = (IConstantValue) Activator.CreateInstance(StandardConstantValues.GetConstantContainerType(valueType));
            _constantValue.Value = value;
        }

        object IAUECustomArgument.GetArgumentValue(IMethodDatabaseOwner methodDbOwner, Type ParameterType, object[] args) => _constantValue.Value;
        bool IAUECustomArgument.IsValid(IMethodDatabaseOwner methodDbOwner, Type ParameterType) => true;
    }
}
