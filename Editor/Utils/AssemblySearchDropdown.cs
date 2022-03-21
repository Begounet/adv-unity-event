using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AUE
{
    public class AssemblySearchDropdown : AdvancedDropdown
    {
        public delegate void AssemblySelectedHandler(SerializedProperty property, Assembly assembly);

        private AssemblySelectedHandler _callback;
        private SerializedProperty _property;

        public AssemblySearchDropdown(SerializedProperty property, AssemblySelectedHandler callback) 
            : base(new AdvancedDropdownState()) 
        {
            _callback = callback;
            _property = property;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Assemblies");
            foreach (var asssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                root.AddChild(new AdvancedDropdownItem(asssembly.GetName().Name));
            }
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            foreach (var asssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asssembly.GetName().Name == item.name)
                {
                    _callback.Invoke(_property, asssembly);
                    return;
                }
            }
            _callback.Invoke(_property, null);
        }
    }
}
