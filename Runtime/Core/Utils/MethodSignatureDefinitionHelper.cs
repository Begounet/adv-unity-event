using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    public static class MethodSignatureDefinitionHelper
    {
        /// <summary>
        /// Set types on argument types, but only if they are different, to dodge allocations.
        /// </summary>
        public static void DefineParameterTypes(List<SerializableType> argumentTypes, params Type[] types)
        {
            bool needsRegenerate = false;
            if (types.Length == argumentTypes.Count)
            {
                for (int i = 0; i < types.Length; ++i)
                {
                    if (types[i] != argumentTypes[i].Type)
                    {
                        needsRegenerate = true;
                        break;
                    }
                }
            }
            else
            {
                needsRegenerate = true;
            }

            if (!needsRegenerate)
            {
                return;
            }

            argumentTypes.Clear();
            argumentTypes.Capacity = types.Length;
            for (int i = 0; i < types.Length; ++i)
            {
                argumentTypes.Add(new SerializableType(types[i]));
            }
        }

        public static void DefineReturnType(SerializableType returnType, Type type)
        {
            if (returnType.IsValidType && returnType.Type == type)
            {
                return;
            }

            returnType.Type = type;
        }
    }
}