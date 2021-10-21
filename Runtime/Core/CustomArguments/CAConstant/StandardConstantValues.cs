using System;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    internal static class StandardConstantValues
    {
        internal static readonly Dictionary<Type, Type> ConstantMapping = new Dictionary<Type, Type>()
        {
            { typeof(UnityEngine.Object), typeof(UnityEngineObjectValue) },
            { typeof(int), typeof(IntValue) },
            { typeof(float), typeof(FloatValue) },
            { typeof(bool), typeof(BoolValue) },
            { typeof(string), typeof(StringValue) },
            { typeof(Vector2), typeof(Vector2Value) },
            { typeof(Vector2Int), typeof(Vector2IntValue) },
            { typeof(Vector3), typeof(Vector3Value) },
            { typeof(Vector3Int), typeof(Vector3IntValue) },
        };

        [Serializable] internal class IntValue : IConstantValue { public int Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class FloatValue : IConstantValue { public float Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class BoolValue : IConstantValue { public bool Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class StringValue : IConstantValue { public string Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class Vector2Value : IConstantValue { public Vector2 Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class Vector2IntValue : IConstantValue { public Vector2Int Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class Vector3Value : IConstantValue { public Vector3 Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class Vector3IntValue : IConstantValue { public Vector3Int Value; object IConstantValue.GetValue() => Value; }
        [Serializable] internal class UnityEngineObjectValue : IConstantValue { public UnityEngine.Object Value; object IConstantValue.GetValue() => Value; }

        [Serializable]
        internal class GenericObject : IConstantValue 
        { 
            [SerializeReference]
            public object Value;

            object IConstantValue.GetValue() => Value;
        }
    }

}
