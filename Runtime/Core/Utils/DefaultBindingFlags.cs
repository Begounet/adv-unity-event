using System.Reflection;
using UnityEngine;

namespace AUE
{
    public static class DefaultBindingFlags
    {
        public const BindingFlags DefaultVisibility =
            BindingFlags.Public
            | BindingFlags.NonPublic;

        public const BindingFlags DefaultScope =
            BindingFlags.Instance
            | BindingFlags.Static;

        public const BindingFlags AUEEvent =
            DefaultVisibility
            | DefaultScope
            | BindingFlags.SetProperty
            | BindingFlags.SetField;

        public const BindingFlags AUESimpleMethod =
            AUEEvent
            | BindingFlags.GetField
            | BindingFlags.GetProperty;

        public const BindingFlags AUEGet =
             DefaultVisibility
            | DefaultScope
            | BindingFlags.GetField
            | BindingFlags.GetProperty;

        public const BindingFlags PrivateFields =
            BindingFlags.NonPublic 
            | BindingFlags.Instance;
    }
}
