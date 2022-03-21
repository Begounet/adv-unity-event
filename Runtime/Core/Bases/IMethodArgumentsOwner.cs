using System;
using System.Collections.Generic;

namespace AUE
{
    internal interface IMethodArgumentsOwner
    {
        IEnumerable<Type> GetArgumentTypes();
    }
}
