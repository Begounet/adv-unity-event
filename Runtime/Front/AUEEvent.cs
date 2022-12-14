using System;
using System.Diagnostics;

namespace AUE
{
    [DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public class AUEEvent : BaseRuntimeAUEEvent<Action>
    {
        public void Invoke()
        {
            base.Invoke();
            RuntimeCallbacks?.Invoke();
        }

        protected override void OnDefineEventsSignature() => DefineParameterTypes();
    }

    [DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public class AUEEvent<T> : BaseRuntimeAUEEvent<Action<T>>
    {
        public void Invoke(T arg)
        {
            base.Invoke(arg);
            RuntimeCallbacks?.Invoke(arg);
        }

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T));
    }

    [DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public class AUEEvent<T0, T1> : BaseRuntimeAUEEvent<Action<T0, T1>>
    {
        public void Invoke(T0 arg1, T1 arg2)
        {
            base.Invoke(arg1, arg2);
            RuntimeCallbacks?.Invoke(arg1, arg2);
        }

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T0), typeof(T1));
    }

    [DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public class AUEEvent<T0, T1, T2> : BaseRuntimeAUEEvent<Action<T0, T1, T2>>
    {
        public void Invoke(T0 arg1, T1 arg2, T2 arg3)
        {
            base.Invoke(arg1, arg2, arg3);
            RuntimeCallbacks?.Invoke(arg1, arg2, arg3);
        }

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T0), typeof(T1), typeof(T2));
    }

    [DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public class AUEEvent<T0, T1, T2, T3> : BaseRuntimeAUEEvent<Action<T0, T1, T2, T3>>
    {
        public void Invoke(T0 arg1, T1 arg2, T2 arg3, T3 arg4)
        {
            base.Invoke(arg1, arg2, arg3, arg4);
            RuntimeCallbacks?.Invoke(arg1, arg2, arg3, arg4);
        }

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T0), typeof(T1), typeof(T2), typeof(T3));
    }

    /// <summary>
    /// Special event whose the type can be changed at runtime.
    /// Allow to pass any arguments. It should match the parameter type.
    /// </summary>
    [DebuggerDisplay("{PrettyName}")]
    [Serializable]
    public class CustomizableAUEEvent : BaseAUEEvent
    {
        public bool SafeInvoke(params object[] args)
        {
            int argIdx = 0;
            foreach (var argType in ArgumentTypes)
            {
                if (!argType.IsAssignableFrom(argIdx.GetType()))
                {
                    return false;
                }
            }
            base.Invoke(args);
            return true;
        }

        public new void Invoke(params object[] args) => base.Invoke(args);
    }
}