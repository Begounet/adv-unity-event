using System;

namespace AUE
{
    [Serializable]
    public class AUEEvent : BaseAUEEvent
    {
        public void Invoke() => base.Invoke();
    }

    [Serializable]
    public class AUEEvent<T> : BaseAUEEvent
    {
        public AUEEvent()
        {
            AddArgumentType(typeof(T));
        }

        public void Invoke(T arg) => base.Invoke(arg);
    }

    [Serializable]
    public class AUEEvent<T0, T1> : BaseAUEEvent
    {
        public AUEEvent()
        {
            AddArgumentType(typeof(T0));
            AddArgumentType(typeof(T1));
        }

        public void Invoke(T0 arg1, T1 arg2) => base.Invoke(arg1, arg2);
    }

    [Serializable]
    public class AUEEvent<T0, T1, T2> : BaseAUEEvent
    {
        public AUEEvent()
        {
            AddArgumentType(typeof(T0));
            AddArgumentType(typeof(T1));
            AddArgumentType(typeof(T2));
        }

        public void Invoke(T0 arg1, T1 arg2, T2 arg3) => base.Invoke(arg1, arg2, arg3);
    }

    [Serializable]
    public class AUEEvent<T0, T1, T2, T3> : BaseAUEEvent
    {
        public AUEEvent()
        {
            AddArgumentType(typeof(T0));
            AddArgumentType(typeof(T1));
            AddArgumentType(typeof(T2));
            AddArgumentType(typeof(T3));
        }

        public void Invoke(T0 arg1, T1 arg2, T2 arg3, T3 arg4) => base.Invoke(arg1, arg2, arg3, arg4);
    }
}