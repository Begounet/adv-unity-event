using System.Reflection;

namespace AUE
{
    public static class DefaultBindingFlags
    {
        public static BindingFlags GetProperty =
            BindingFlags.GetProperty 
            | BindingFlags.Public 
            | BindingFlags.NonPublic 
            | BindingFlags.Instance;

        public static BindingFlags AUEGet =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.GetProperty
            | BindingFlags.GetField;

        public static BindingFlags AUEEvent =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.SetProperty
            | BindingFlags.SetField;

        public static BindingFlags AUESimpleMethod =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty
            | BindingFlags.SetField;
    }
}
