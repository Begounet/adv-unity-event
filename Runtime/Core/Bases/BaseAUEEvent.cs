using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace AUE
{
    public class BaseAUEEvent : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<AUEMethod> _events = new List<AUEMethod>();

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

        public bool IsBound => (_events?.Count > 0);

        protected void Invoke(params object[] args)
        {
            for (int i = 0; i < _events.Count; ++i)
            {
                _events[i].Invoke(args);
            }
        }

        public void DefineParameterTypes(params Type[] types)
        {
            MethodSignatureDefinitionHelper.DefineParameterTypes(_argumentTypes, types);
        }

        public void AddEvent(AUEMethod method)
        {
            _events.Add(method);
        }

        public virtual void OnBeforeSerialize()
        {
            SynchronizeToEvents();
            OnDefineEventsSignature();
        }

        public virtual void OnAfterDeserialize()
        {
            SynchronizeToEvents();
        }

        protected virtual void OnDefineEventsSignature() { }

        private void SynchronizeToEvents()
        {
            if (_events != null)
            {
                for (int i = 0; i < _events.Count; ++i)
                {
                    _events[i].BindingFlags = _bindingFlags;
                }
            }
        }
    }
}