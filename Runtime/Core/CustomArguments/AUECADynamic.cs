using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AUE
{
    [Serializable]
    public class AUECADynamic : IAUECustomArgument
    {
        [SerializeField]
        private int _sourceArgumentIndex = 0;

        object IAUECustomArgument.GetArgumentValue(IMethodDatabaseOwner methodDbOwner, Type ParameterType, object[] args)
        {
            if (_sourceArgumentIndex >= 0 && _sourceArgumentIndex <= args.Length &&
               DoesParameterTypeMatch(args[_sourceArgumentIndex], ParameterType))
            {
                return args[_sourceArgumentIndex];
            }
            return null;
        }

        bool IAUECustomArgument.IsValid(IMethodDatabaseOwner methodDbOwner, Type ParameterType) => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DoesParameterTypeMatch(object arg, Type parameterType)
            // If argument is null, we don't care about the parameter type
            => (arg == null || parameterType.IsAssignableFrom(arg.GetType()));
    }
}
