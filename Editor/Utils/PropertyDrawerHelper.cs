using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace AUE
{
    public static class PropertyDrawerHelper
    {
        private static readonly Type ScriptAttributeUtilityType = typeof(EditorGUI).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
        private static readonly Type PropertyHandlerType = typeof(EditorGUI).Assembly.GetType("UnityEditor.PropertyHandler");

        private static readonly MethodInfo GetHandlerMethodInfo = ScriptAttributeUtilityType.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo GetPropertyDrawersFieldInfo = PropertyHandlerType.GetField("m_PropertyDrawers", BindingFlags.Instance | BindingFlags.NonPublic);

        public class IndentedLevelResetScope : IDisposable
        {
            private int _indentedLevel;

            public IndentedLevelResetScope()
            {
                _indentedLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = _indentedLevel;
            }
        }

        public static PropertyDrawer GetPropertyDrawer(this SerializedProperty sp)
        {
            var pds = GetPropertyDrawers(sp);
            return (pds.Count > 0 ? pds[0] : null);
        }

        public static List<PropertyDrawer> GetPropertyDrawers(this SerializedProperty sp)
        {
            var handler = GetHandlerMethodInfo.Invoke(null, new object[] { sp });
            return (List<PropertyDrawer>)GetPropertyDrawersFieldInfo.GetValue(handler);
        }
    }
}