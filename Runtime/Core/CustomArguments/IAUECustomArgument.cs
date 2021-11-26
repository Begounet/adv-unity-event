using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    public interface IAUECustomArgument
    {
        internal object GetArgumentValue(IAUEMethod aueMethod, Type ParameterType, object[] args);
    }
}
