using UnityEngine;

namespace AUE
{
    internal class AUECustomMethodExecutionCache : IMethodExecutionCache
    {
        object IMethodExecutionCache.Invoke(IMethodDatabaseOwner methodDbOwner, params object[] args)
        {
            // Replace parameter
            return null;
        }
    }
}