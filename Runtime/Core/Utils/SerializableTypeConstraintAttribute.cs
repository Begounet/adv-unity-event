using System;
using UnityEngine;

namespace AUE
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SerializableTypeConstraintAttribute : PropertyAttribute
    {
        public Type ConstraintType { get; set; } = null;

        public ETypeUsageFlag FilterFlags { get; set; } 
            = ETypeUsageFlag.Class
            | ETypeUsageFlag.Struct
            | ETypeUsageFlag.Interface
            | ETypeUsageFlag.Abstract;

        public SerializableTypeConstraintAttribute() { }

        public SerializableTypeConstraintAttribute(Type constraintParentType)
        {
            ConstraintType = constraintParentType;
        }
    }
}
