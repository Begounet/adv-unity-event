using System;

namespace AUE
{
    [Serializable]
    public class AUEEvent : BaseAUEEvent
    {
        public void Invoke() => base.Invoke();

        protected override void OnDefineEventsSignature() => DefineParameterTypes();
    }

    [Serializable]
    public class AUEEvent<T> : BaseAUEEvent
    {
        public void Invoke(T arg) => base.Invoke(arg);

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T));
    }

    [Serializable]
    public class AUEEvent<T0, T1> : BaseAUEEvent
    {
        public void Invoke(T0 arg1, T1 arg2) => base.Invoke(arg1, arg2);

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T0), typeof(T1));
    }

    [Serializable]
    public class AUEEvent<T0, T1, T2> : BaseAUEEvent
    {
        public void Invoke(T0 arg1, T1 arg2, T2 arg3) => base.Invoke(arg1, arg2, arg3);

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T0), typeof(T1), typeof(T2));
    }

    [Serializable]
    public class AUEEvent<T0, T1, T2, T3> : BaseAUEEvent
    {
        public void Invoke(T0 arg1, T1 arg2, T2 arg3, T3 arg4) => base.Invoke(arg1, arg2, arg3, arg4);

        protected override void OnDefineEventsSignature() => DefineParameterTypes(typeof(T0), typeof(T1), typeof(T2), typeof(T3));
    }
}