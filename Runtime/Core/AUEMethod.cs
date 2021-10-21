using System;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    [Serializable]
    public class AUEMethod : AUESimpleMethod, IMethodDatabaseOwner
    {
        [SerializeField]
        private List<SerializableType> _argumentTypes = new List<SerializableType>();
        public List<SerializableType> ArgumentTypes
        {
            get => _argumentTypes;
            set => _argumentTypes = value;
        }

        [SerializeField]
        private AUESimpleMethod[] _methodDatabase;
        IList<AUESimpleMethod> IMethodDatabaseOwner.MethodDatabase => _methodDatabase;

        internal object Invoke(params object[] args)
        {
            return base.Invoke(this, args);
        }
    }
}