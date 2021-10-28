using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AUE
{
    public class TypeSearchDropdown : AdvancedDropdown
    {
        private static Dictionary<Assembly, Type[]> _typesCache = null;

        public event Action<Type> OnTypeSelected;

        public TypeSearchDropdown(AdvancedDropdownState state)
            : base(state)
        {
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
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                _typesCache.Add(assembly, assembly.GetTypes());
            }
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
