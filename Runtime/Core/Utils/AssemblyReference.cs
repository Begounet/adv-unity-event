using UnityEngine;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AUE
{
    [Serializable]
    public class AssemblyReference
    {
        [SerializeField]
        private string _assemblyName;
        public string AssemblyName
        {
            get => _assemblyName;
            set
            {
                _assemblyName = value;
                _isDirty = true;
            }
        }

        private Assembly _assembly;
        public Assembly Assembly
        { 
            get
            {
                if (_isDirty)
                {
                    _assembly = FindAssemblyByName(_assemblyName);
                    _isDirty = false;
                }
                return _assembly;
            }
            set
            {
                _assembly = value;
                _assemblyName = GetAssemblyName(_assembly);
                _isDirty = false;
            }
        }

        private bool _isDirty = true;

        public AssemblyReference() { }
        public AssemblyReference(Type t) => Assembly = t.Assembly;
        public AssemblyReference(Assembly assembly) => Assembly = assembly;

        public static string GetAssemblyName(Type type) => GetAssemblyName(type.Assembly);
        public static string GetAssemblyName(Assembly assembly) => assembly.FullName;

        public static Assembly FindAssemblyByName(string assemblyName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (GetAssemblyName(assembly) == assemblyName)
                {
                    return assembly;
                }
            }
            return null;
        }
    }
}
