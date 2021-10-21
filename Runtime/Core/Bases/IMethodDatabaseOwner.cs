using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    internal interface IMethodDatabaseOwner
    {
        IList<AUESimpleMethod> MethodDatabase { get; }
    }
}