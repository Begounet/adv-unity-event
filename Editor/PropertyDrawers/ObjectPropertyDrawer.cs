using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    public class ObjectPropertyDrawer
    {
        private static Dictionary<Type, PropertyDrawer> _propertyDrawHandlers = new Dictionary<Type, PropertyDrawer>();

        public void Draw(Rect position, FieldInfo fi, object value, Type type)
        {
            var pd = GetOrCreatePropertyDrawer(fi, type);
            if (pd != null)
            {
                //pd.OnGUI(position, )
            }
        }

        private static Type GetDrawerTypeForType(Type type)
        {
            Type scriptAttributeUtility = Type.GetType("UnityEditor.ScriptAttributeUtility, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            MethodInfo getDrawerTypeForTypeMI = scriptAttributeUtility.GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic);
            return (Type) getDrawerTypeForTypeMI.Invoke(null, new object[] { type });
        }

        private PropertyDrawer GetOrCreatePropertyDrawer(FieldInfo fi, Type type)
        {
            if (_propertyDrawHandlers.TryGetValue(type, out PropertyDrawer propertyDrawer))
            {
                Type drawerType = GetDrawerTypeForType(type);
                if (typeof(PropertyDrawer).IsAssignableFrom(drawerType))
                {
                    propertyDrawer = InstantiatePropertyDrawer(drawerType, fi);
                    _propertyDrawHandlers.Add(type, propertyDrawer);
                }
            }
            return propertyDrawer;
        }

        private PropertyDrawer InstantiatePropertyDrawer(Type drawerType, FieldInfo fi)
        {
            PropertyDrawer pd = (PropertyDrawer) Activator.CreateInstance(drawerType);
            typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pd, fi);
            return pd;
        }
    }
}