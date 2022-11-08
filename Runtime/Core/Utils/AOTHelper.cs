#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AUE
{
    public static class AOTHelper
    {
        internal static void RegisterMemberInfosInPropertyPath(HashSet<MemberInfo> outPropertyPathMembers, Type targetType, string propertyPath)
        {
            string[] propertyPathItems = propertyPath.Split('.');
            RegisterMemberInfosInPropertyPathRecursive(outPropertyPathMembers, targetType, propertyPathItems, 0);
        }

        private static void RegisterMemberInfosInPropertyPathRecursive(HashSet<MemberInfo> outPropertyPathMembers, Type targetType, string[] propertyPath, int startIndex)
        {
            BindingFlags bf = DefaultBindingFlags.GetProperty;

            string propertyName = propertyPath[startIndex];
            Type propertyType = null;

            var propertyInfo = targetType.GetProperty(propertyName, bf);
            if (propertyInfo != null)
            {
                propertyType = propertyInfo.PropertyType;
                outPropertyPathMembers.Add(propertyInfo);
            }
            else
            {
                var fieldInfo = targetType.GetField(propertyName, bf);
                if (fieldInfo != null)
                {
                    propertyType = fieldInfo.FieldType;
                    outPropertyPathMembers.Add(fieldInfo);
                }
            }

            if (propertyType == null || startIndex + 1 == propertyPath.Length)
            {
                return;
            }

            RegisterMemberInfosInPropertyPathRecursive(outPropertyPathMembers, propertyType, propertyPath, startIndex + 1);
        }
    }
}
#endif