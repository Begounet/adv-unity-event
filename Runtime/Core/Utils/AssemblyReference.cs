using UnityEngine;
using System;
using System.Reflection;

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

        private string _assemblyShortName;
        public string AssemblyShortName 
        { 
            get
            {
                CacheAssemblyIFN();
                return _assemblyShortName;
            }
        }


        private Assembly _assembly;
        public Assembly Assembly
        { 
            get
            {
                CacheAssemblyIFN();
                return _assembly;
            }
            set
            {
                _assembly = value;
                CacheAssemblyIFN();
            }
        }

        private bool _isDirty = true;

        public AssemblyReference() { }
        public AssemblyReference(Type t) => Assembly = t.Assembly;
        public AssemblyReference(Assembly assembly) => Assembly = assembly;

        public static string GetAssemblyName(Type type) => GetAssemblyName(type.Assembly);
        public static string GetAssemblyName(Assembly assembly) => assembly.FullName;

        private void CacheAssemblyIFN()
        {

            if (_isDirty)
            {
                _assembly = FindAssemblyByName(_assemblyName);
                _assemblyShortName = _assembly?.GetName().Name ?? string.Empty;
                _isDirty = false;
            }
        }
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
