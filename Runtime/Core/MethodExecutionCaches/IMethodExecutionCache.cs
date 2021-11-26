namespace AUE
{
    internal interface IMethodExecutionCache
    {
        internal object Invoke(IAUEMethod aueMethod, params object[] args);
    }
}