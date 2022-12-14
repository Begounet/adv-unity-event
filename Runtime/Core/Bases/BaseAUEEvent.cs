using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityEngine.Pool;
using System.Text;

namespace AUE
{
    [System.Diagnostics.DebuggerDisplay("{PrettyName}")]
    public class BaseAUEEvent : ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<AUEMethod> _events = new List<AUEMethod>();
        public IReadOnlyList<AUEMethod> Events => _events.AsReadOnly();

        [SerializeField]
        private List<SerializableType> _argumentTypes = new List<SerializableType>();
        public IEnumerable<Type> ArgumentTypes => _argumentTypes.Select((argType) => argType.Type);

        [SerializeField]
        private BindingFlags _bindingFlags = DefaultBindingFlags.AUEEvent;
        public BindingFlags BindingFlags
        {
            get => _bindingFlags;
            set
            {
                _bindingFlags = value;
                SynchronizeToEvents();
            }
        }

        public virtual bool IsBound => (_events?.Count > 0);

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public string PrettyName => GeneratePrettyName();

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

        public void ClearEvents()
        {
            _events.Clear();
        }

        public virtual void OnBeforeSerialize()
        {
            InitBindingFlagsIFN();
            SynchronizeToEvents();
            OnDefineEventsSignature();
        }

        public virtual void OnAfterDeserialize()
        {
            InitBindingFlagsIFN();
            SynchronizeToEvents();
        }

        /// <summary>
        /// Exists because of Unity bad serialization.
        /// When created as an array item, serialized fields are not
        /// correctly initialized to their default values.
        /// So we have to ensure it ourselves.
        /// </summary>
        private void InitBindingFlagsIFN()
        {
            if (_bindingFlags == BindingFlags.Default)
            {
                _bindingFlags = DefaultBindingFlags.AUEEvent;
            }
        }

        protected virtual void OnDefineEventsSignature() { }

        private void SynchronizeToEvents()
        {
            if (_events != null)
            {
                for (int i = 0; i < _events.Count; ++i)
                {
                    _events[i].BindingFlags = _bindingFlags;
                    _events[i].ReturnType = null; // Temporary : should not be required, but in previous implementation, return type was void, being wrong stuff
                }
            }
        }

        protected virtual string GeneratePrettyName()
        {
            var sb = UnsafeGenericPool<StringBuilder>.Get();
            sb.Clear();
            {
                if (_events.Count == 0)
                {
                    sb.Append("No events");
                }
                else
                {
                    int maxEventsDisplayed = 4;
                    for (int i = 0; i < _events.Count && i < maxEventsDisplayed; ++i)
                    {
                        sb.Append(_events[i].PrettyName);
                        if (i + 1 < _events.Count)
                        {
                            sb.Append(" | ");
                        }
                        if (i + 1 == maxEventsDisplayed)
                        {
                            sb.Append('…');
                        }
                    }
                }
            }
            string result = sb.ToString();
            UnsafeGenericPool<StringBuilder>.Release(sb);
            return result;
        }
    }
}