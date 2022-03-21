using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AssemblyReference))]
    public class AssemblyReferencePropertyDrawer : PropertyDrawer
    {
        private const string AssemblyNameSPName = "_assemblyName";
        private Dictionary<string, Assembly> _assemblyChangesQueue = new Dictionary<string, Assembly>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assemblyNameSP = property.FindPropertyRelative(AssemblyNameSPName);
            Assembly assembly = AssemblyReference.FindAssemblyByName(assemblyNameSP.stringValue);
            GUIContent name = new GUIContent((assembly?.GetName().Name ?? "<undefined>"));
            if (EditorGUI.DropdownButton(position, name, FocusType.Keyboard))
            {
                new AssemblySearchDropdown(property, (property, assembly) =>
                {
                    _assemblyChangesQueue.Add(property.propertyPath, assembly);
                    GUI.changed = true;
                }).Show(position);
            }

            if (_assemblyChangesQueue != null)
            {
                if (_assemblyChangesQueue.TryGetValue(property.propertyPath, out assembly))
                {
                    assemblyNameSP.stringValue = AssemblyReference.GetAssemblyName(assembly);
                    _assemblyChangesQueue.Remove(property.propertyPath);
                    GUI.changed = true;
                }
            }
        }
    }
}

