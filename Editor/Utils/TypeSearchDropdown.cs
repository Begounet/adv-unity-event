using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AUE
{
    public class TypeSearchDropdown : AdvancedDropdown
    {
        public class Settings
        {
            public Type ConstraintType { get; set; }
            public ETypeUsageFlag UsageFlags { get; set; }
        }

        private static Dictionary<Assembly, Type[]> _typesCache = null;

        private Settings _customSettings = null;

        public event Action<Type> OnTypeSelected;


        public TypeSearchDropdown(AdvancedDropdownState state, Settings customSettings = null)
            : base(state)
        {
            minimumSize = new Vector2(200, 300);
            _customSettings = customSettings;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("root");
            CacheTypesIFN();
            foreach (var kv in _typesCache)
            {
                var assemblyItem = new AdvancedDropdownItem(kv.Key.GetName().Name) { id = kv.Key.GetHashCode() };
                int count = kv.Value.Length;
                for (int i = 0; i < count; ++i)
                {
                    var type = kv.Value[i];
                    assemblyItem.AddChild(new AdvancedDropdownItem(type.FullName) { id = type.GetHashCode() });
                }
                root.AddChild(assemblyItem);
            }
            root.AddSeparator();
            root.AddChild(new AdvancedDropdownItem("Undefined") { id = 0 });
            return root;
        }

        private void CacheTypesIFN()
        {
            if (_typesCache != null)
            {
                return;
            }

            _typesCache = new Dictionary<Assembly, Type[]>();
            var aueSettings = AUESettingsProvider.GetOrCreateSettings<AUESettings>();
            var assemblyReferences = aueSettings.TypesAssemblies;
            foreach (var assemblyRef in assemblyReferences)
            {
                var assembly = assemblyRef.Assembly;
                if (assembly != null && !_typesCache.ContainsKey(assembly))
                {
                    if (_customSettings != null)
                    {
                        Type[] types = GetConstraintTypesFromAssembly(assembly, _customSettings);
                        if (types != null && types.Length > 0)
                        {
                            _typesCache.Add(assembly, types);
                        }
                    }
                    else
                    {
                        _typesCache.Add(assembly, assembly.GetTypes());
                    }
                }
            }
        }

        private Type[] GetConstraintTypesFromAssembly(Assembly assembly, Settings settings)
        {
            Type[] types = assembly.GetTypes();
            return types
                .Where((t) 
                => (settings.ConstraintType == null || settings.ConstraintType.IsAssignableFrom(t))
                && (!t.IsAbstract || settings.UsageFlags.HasFlag(ETypeUsageFlag.Abstract))
                && (!t.IsInterface || settings.UsageFlags.HasFlag(ETypeUsageFlag.Interface))
                && (!t.IsClass || settings.UsageFlags.HasFlag(ETypeUsageFlag.Class))
                && (!t.IsValueType || settings.UsageFlags.HasFlag(ETypeUsageFlag.Struct)))
                .ToArray();
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            Type selectedType = FindItemById(item.id);
            OnTypeSelected.Invoke(selectedType);
        }

        private Type FindItemById(int id)
        {
            if (id == 0)
            {
                return null;
            }
            foreach (var kv in _typesCache)
            {
                var assemblyItem = new AdvancedDropdownItem(kv.Key.GetName().Name) { id = kv.Key.GetHashCode() };
                int count = kv.Value.Length;
                for (int i = 0; i < count; ++i)
                {
                    var type = kv.Value[i];
                    if (type.GetHashCode() == id)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}
