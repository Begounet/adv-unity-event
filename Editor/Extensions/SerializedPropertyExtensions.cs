using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace AUE
{
    public static class SerializedPropertyExtentions
    {
        private static Regex ArrayIndexCapturePattern = new Regex(@"\[(\d*)\]");

        public static object GetTarget(this SerializedProperty prop)
        {
            string[] propertyNames = prop.propertyPath.Split('.');
            object target = prop.serializedObject.targetObject;
            bool isNextPropertyArrayIndex = false;
            for (int i = 0; i < propertyNames.Length && target != null; ++i)
            {
                string propName = propertyNames[i];
                if (propName == "Array")
                {
                    isNextPropertyArrayIndex = true;
                }
                else if (isNextPropertyArrayIndex)
                {
                    isNextPropertyArrayIndex = false;
                    int arrayIndex = ParseArrayIndex(propName);
                    var targetAsArray = (IList)target;
                    if (arrayIndex < 0 || arrayIndex >= targetAsArray.Count)
                    {
                        return default;
                    }

                    target = targetAsArray[arrayIndex];
                }
                else
                {
                    target = GetField(target, propName);
                }
            }
            return target;
        }

        public static T GetTarget<T>(this SerializedProperty prop)
            => (T)GetTarget(prop);

        private static object GetField(object target, string name, Type targetType = null)
        {
            if (targetType == null)
            {
                targetType = target.GetType();
            }

            FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | DefaultBindingFlags.DefaultVisibility);
            if (fi != null)
            {
                return fi.GetValue(target);
            }

            // If not found, search in parent
            if (targetType.BaseType != null)
            {
                return GetField(target, name, targetType.BaseType);
            }
            return null;
        }

        private static int ParseArrayIndex(string propName)
        {
            Match match = ArrayIndexCapturePattern.Match(propName);
            if (!match.Success)
            {
                throw new Exception($"Invalid array index parsing in {propName}");
            }

            return int.Parse(match.Groups[1].Value);
        }

        public static FieldInfo GetField(SerializedObject so, string fieldPath)
        {
            string[] propertyNames = fieldPath.Split('.');
            object target = so.targetObject;
            bool isNextPropertyArrayIndex = false;
            for (int i = 0; i < propertyNames.Length - 1 && target != null; ++i)
            {
                string propName = propertyNames[i];
                if (propName == "Array")
                {
                    isNextPropertyArrayIndex = true;
                }
                else if (isNextPropertyArrayIndex)
                {
                    isNextPropertyArrayIndex = false;
                    int arrayIndex = ParseArrayIndex(propName);
                    object[] targetAsArray = (object[])target;
                    target = targetAsArray[arrayIndex];
                }
                else
                {
                    target = GetField(target, propName);
                }
            }
            
            if (target != null)
            {
                return GetFieldInfo(target, propertyNames[propertyNames.Length - 1]);
            }
            return null;
        }

        private static FieldInfo GetFieldInfo(object target, string name, Type targetType = null)
        {
            if (targetType == null)
            {
                targetType = target.GetType();
            }

            FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | DefaultBindingFlags.DefaultVisibility);
            if (fi != null)
            {
                return fi;
            }

            // If not found, search in parent
            if (targetType.BaseType != null)
            {
                return GetFieldInfo(target, name, targetType.BaseType);
            }
            return null;
        }

        public static byte[] GetBytesArray(this SerializedProperty property)
        {
            if (!property.isArray || property.arrayElementType != "byte")
            {
                return null;
            }

            byte[] arr = new byte[property.arraySize];
            for (int i = 0; i < property.arraySize; ++i)
            {
                arr[i] = (byte) property.GetArrayElementAtIndex(i).intValue;
            }
            return arr;
        }

        public static void SetBytesArray(this SerializedProperty property, byte[] array)
        {
            if (!property.isArray || property.arrayElementType != "byte")
            {
                return;
            }

            property.arraySize = array.Length;
            for (int i = 0; i < array.Length; ++i)
            {
                property.GetArrayElementAtIndex(i).intValue = array[i];
            }
        }

        public static SerializedProperty GetParent(this SerializedProperty property)
        {
            string path = property.propertyPath;
            if (path.EndsWith("]"))
            {
                // If is array, remove last part twice (`Array.data[x]`)
                path = path.Remove(path.LastIndexOf('.'));
            }
            path = path.Remove(path.LastIndexOf('.'));
            return property.serializedObject.FindProperty(path);
        }

        public static bool HasProperty(this SerializedProperty property, string propertyName) 
            => (property.FindPropertyRelative(propertyName) != null);
    }
}