using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace AUE
{
    public class BaseAUEEvent : ISerializationCallbackReceiver
    {
        [SerializeField]
        private AUEMethod[] _events;

        [SerializeField]
        private SerializableType _returnType;
        public Type ReturnType
        {
            get => _returnType.IsValidType ? _returnType.Type : null;
            set
            {
                _returnType.Type = value;
                SynchronizeToEvents();
            }
        }

        [SerializeField]
        private List<SerializableType> _argumentTypes = new List<SerializableType>();

        [SerializeField]
        private BindingFlags _bindingFlags =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.SetProperty
            | BindingFlags.SetField;
        public BindingFlags BindingFlags
        {
            get => _bindingFlags;
            set
            {
                _bindingFlags = value;
                SynchronizeToEvents();
            }
        }

        public bool IsBound => (_events.Length > 0);

        protected void Invoke(params object[] args)
        {
            for (int i = 0; i < _events.Length; ++i)
            {
                _events[i].Invoke(args);
            }
        }

        public void SetReturnType(Type type)
        {
            _returnType.Type = type;
        }

        public void AddArgumentType(Type type)
        {
            _argumentTypes.Add(new SerializableType(type));
        }

        public void OnBeforeSerialize()
        {
            SynchronizeToEvents();
        }

        public void OnAfterDeserialize()
        {
            SynchronizeToEvents();
        }

        private void SynchronizeToEvents()
        {
            if (_events != null)
            {
                for (int i = 0; i < _events.Length; ++i)
                {
                    _events[i].ReturnType = (_returnType.IsValidType ? _returnType.Type : null);
                    _events[i].BindingFlags = _bindingFlags;
                }
            }
        }
    }
}