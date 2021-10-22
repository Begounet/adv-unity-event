using System;

namespace AUE
{
    [Serializable]
	public class AUEGet<TResult> : BaseAUEGet
	{
		public TResult Invoke() => Invoke<TResult>();

		protected override void OnDefineSignatureMethod() 
			=> DefineReturnAndParametersType(typeof(TResult));
    }

	[Serializable]
	public class AUEGet<T0, TResult> : BaseAUEGet
	{
		public TResult Invoke(T0 arg0) => Invoke<TResult>(arg0);

		protected override void OnDefineSignatureMethod() 
			=> DefineReturnAndParametersType(typeof(TResult), typeof(T0));
	}

	[Serializable]
	public class AUEGet<T0, T1, TResult> : BaseAUEGet
	{
		public TResult Invoke(T0 arg0, T1 arg1) => Invoke<TResult>(arg0, arg1);

		protected override void OnDefineSignatureMethod()
			=> DefineReturnAndParametersType(typeof(TResult), typeof(T0), typeof(T1));
	}

	[Serializable]
	public class AUEGet<T0, T1, T2, TResult> : BaseAUEGet
	{
		public TResult Invoke(T0 arg0, T1 arg1, T2 arg2) => Invoke<TResult>(arg0, arg1, arg2);

		protected override void OnDefineSignatureMethod()
			=> DefineReturnAndParametersType(typeof(TResult), typeof(T0), typeof(T1), typeof(T2));
	}

	[Serializable]
	public class AUEGet<T0, T1, T2, T3, TResult> : BaseAUEGet
	{
		public TResult Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3) => Invoke<TResult>(arg0, arg1, arg2, arg3);

		protected override void OnDefineSignatureMethod()
			=> DefineReturnAndParametersType(
				typeof(TResult),
				typeof(T0),
				typeof(T1), 
				typeof(T2),
				typeof(T3));
	}
}