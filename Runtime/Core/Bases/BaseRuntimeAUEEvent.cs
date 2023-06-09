using System;
using System.Diagnostics;

namespace AUE
{
    [DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public abstract class BaseRuntimeAUEEvent<TDelegate> : BaseAUEEvent
        where TDelegate : Delegate
    {
        protected TDelegate RuntimeCallbacks;

        public void AddAction(TDelegate callback) => RuntimeCallbacks = (TDelegate)Delegate.Combine(RuntimeCallbacks, callback);
        public void RemoveAction(TDelegate callback) => RuntimeCallbacks = (TDelegate)Delegate.Remove(RuntimeCallbacks, callback);

        public override bool IsBound => base.IsBound || (RuntimeCallbacks != null);

        public override string PrettyName
        {
            get
            {
                if (RuntimeCallbacks != null)
                {
                    Delegate[] runtimeInvokeList = RuntimeCallbacks.GetInvocationList();
                    return PrettyNameHelper.GeneratePrettyName(this, runtimeInvokeList);
                }
                return base.PrettyName;
            }
        }
    }
}
