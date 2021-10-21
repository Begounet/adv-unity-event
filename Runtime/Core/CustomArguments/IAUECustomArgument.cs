using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    public interface IAUECustomArgument
    {
        internal object GetArgumentValue(IMethodDatabaseOwner methodDbOwner, Type ParameterType, object[] args);
        internal bool IsValid(IMethodDatabaseOwner methodDbOwner, Type ParameterType);
    }
}
