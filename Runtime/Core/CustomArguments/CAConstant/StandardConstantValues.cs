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

        [Serializable]
        internal class IntValue : IConstantValue 
        {
            [SerializeField]
            private int _value;
            object IConstantValue.Value { get => _value; set => _value = (int)value; }
        }

        [Serializable]
        internal class FloatValue : IConstantValue
        {
            [SerializeField]
            private float _value;
            object IConstantValue.Value { get => _value; set => _value = (float)value; }
        }

        [Serializable] 
        internal class BoolValue : IConstantValue 
        {
            [SerializeField]
            private bool _value;
            object IConstantValue.Value { get => _value; set => _value = (bool)value; }
        }

        [Serializable]
        internal class StringValue : IConstantValue 
        {
            [SerializeField]
            private string _value;
            object IConstantValue.Value { get => _value; set => _value = (string)value; }
        }

        [Serializable] 
        internal class Vector2Value : IConstantValue 
        {
            [SerializeField]
            private Vector2 _value;
            object IConstantValue.Value { get => _value; set => _value = (Vector2)value; }
        }

        [Serializable]
        internal class Vector2IntValue : IConstantValue 
        {
            [SerializeField]
            private Vector2Int _value;
            object IConstantValue.Value { get => _value; set => _value = (Vector2Int)value; }
        }
        
        [Serializable] 
        internal class Vector3Value : IConstantValue 
        {
            [SerializeField]
            private Vector3 _value;
            object IConstantValue.Value { get => _value; set => _value = (Vector3)value; }
        }

        [Serializable] 
        internal class Vector3IntValue : IConstantValue 
        {
            [SerializeField]
            private Vector3Int _value;
            object IConstantValue.Value { get => _value; set => _value = (Vector3Int)value; }
        }
        
        [Serializable] 
        internal class UnityEngineObjectValue : IConstantValue 
        {
            [SerializeField]
            private UnityEngine.Object _value;
            object IConstantValue.Value { get => _value; set => _value = (UnityEngine.Object)value; }
        }

        [Serializable]
        internal class GenericObject : IConstantValue 
        { 
            [SerializeReference]
            private object _value;

            object IConstantValue.Value { get => _value; set => _value = value; }
        }

        public static Type GetConstantContainerType(Type t)
        {
            foreach (var constantValue in ConstantMapping)
            {
                if (constantValue.Key.IsAssignableFrom(t))
                {
                    return constantValue.Value;
                }
            }
            return typeof(GenericObject);
        }
    }

}
