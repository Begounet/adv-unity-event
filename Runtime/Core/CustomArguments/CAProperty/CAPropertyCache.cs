using UnityEngine;
using System;
using System.Reflection;

namespace AUE
{
    public class CAPropertyCache
    {
        private interface ICacheItem
        {
            object GetValue(object target);
        }

        private class PropertyCacheAccess : ICacheItem
        {
            private readonly PropertyInfo _pi;

            public PropertyCacheAccess(PropertyInfo pi) { _pi = pi; }
            public object GetValue(object target) => _pi.GetValue(target);
        }

        private class FieldCacheAccess : ICacheItem
        {
            private readonly FieldInfo _fi;

            public FieldCacheAccess(FieldInfo fi) { _fi = fi; }
            public object GetValue(object target) => _fi.GetValue(target);
        }

        private ICacheItem[] _items;
        public bool IsValid { get; private set; }

        public object GetValue(AUECAProperty.EExecutionSafeMode executionSafeMode, object target)
        {
            if (!IsValid)
            {
                return null;
            }

            try
            {
                for (int i = 0; i < _items.Length; ++i)
                {
                    target = _items[i].GetValue(target);
                }
                return target;
            }
            catch (Exception ex)
            {
                switch (executionSafeMode)
                {
                    case AUECAProperty.EExecutionSafeMode.Default:
                        return null;
                    default:
                    case AUECAProperty.EExecutionSafeMode.Unsafe:
                        throw ex;
                }
            }
        }

        public void BuildCache(Type targetType, string propertyPath)
        {
            string[] propertyChunks = propertyPath.Split('.');
            _items = new ICacheItem[propertyChunks.Length];
            IsValid = BuildCacheRecursive(targetType, propertyChunks, 0);
            if (!IsValid)
            {
                // If invalid, release memory
                _items = null;
            }
        }

        private bool BuildCacheRecursive(Type targetType, string[] propertyPath, int startIndex)
        {
            BindingFlags bf = DefaultBindingFlags.DefaultVisibility | BindingFlags.GetProperty | BindingFlags.Instance;

            string propertyName = propertyPath[startIndex];
            Type propertyType = null;

            var propertyInfo = targetType.GetProperty(propertyName, bf);
            if (propertyInfo != null)
            {
                propertyType = propertyInfo.PropertyType;
                _items[startIndex] = new PropertyCacheAccess(propertyInfo);
            }
            else
            {
                var fieldInfo = targetType.GetField(propertyName, bf);
                if (fieldInfo != null)
                {
                    propertyType = fieldInfo.FieldType;
                    _items[startIndex] = new FieldCacheAccess(fieldInfo);
                }
            }

            if (propertyType == null)
            {
                return false;
            }
            if (startIndex + 1 == propertyPath.Length)
            {
                return true;
            }

            return BuildCacheRecursive(propertyType, propertyPath, startIndex + 1);
        }
    }
}
