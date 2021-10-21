namespace AUE
{
    internal interface IMethodExecutionCache
    {
        internal object Invoke(IMethodDatabaseOwner methodDbOwner, params object[] args);
    }
}