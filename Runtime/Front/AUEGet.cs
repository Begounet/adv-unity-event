using System;

namespace AUE
{
    [Serializable]
	public class AUEGet<TResult> : BaseAUEGet
	{
		public AUEGet() => SetReturnType(typeof(TResult));
		public TResult Invoke() => base.Invoke<TResult>();
	}

	[Serializable]
	public class AUEGet<T0, TResult> : BaseAUEGet
	{
		public AUEGet()
		{
			SetReturnType(typeof(TResult));
			AddArgumentType(typeof(T0));
		}

		public TResult Invoke(T0 arg0) => base.Invoke<TResult>(arg0);
	}

	[Serializable]
	public class AUEGet<T0, T1, TResult> : BaseAUEGet
	{
		public AUEGet()
		{
			SetReturnType(typeof(TResult));
			AddArgumentType(typeof(T0));
			AddArgumentType(typeof(T1));
		}

		public TResult Invoke(T0 arg0, T1 arg1) => base.Invoke<TResult>(arg0, arg1);
	}

	[Serializable]
	public class AUEGet<T0, T1, T2, TResult> : BaseAUEGet
	{
		public AUEGet()
		{
			SetReturnType(typeof(TResult));
			AddArgumentType(typeof(T0));
			AddArgumentType(typeof(T1));
			AddArgumentType(typeof(T2));
		}

		public TResult Invoke(T0 arg0, T1 arg1, T2 arg2) => base.Invoke<TResult>(arg0, arg1, arg2);
	}

	[Serializable]
	public class AUEGet<T0, T1, T2, T3, TResult> : BaseAUEGet
	{
		public AUEGet()
		{
			SetReturnType(typeof(TResult));
			AddArgumentType(typeof(T0));
			AddArgumentType(typeof(T1));
			AddArgumentType(typeof(T2));
			AddArgumentType(typeof(T3));
		}

		public TResult Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3) => base.Invoke<TResult>(arg0, arg1, arg2, arg3);
	}
}