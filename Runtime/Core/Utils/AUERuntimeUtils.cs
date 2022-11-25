using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AUE
{
    public static class AUERuntimeUtils
    {
        private static readonly FieldInfo InstanceIDFieldInfo = typeof(Object).GetField("m_InstanceID", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsUnityObjectValid(Object obj)
        {
            int instanceID = (int)InstanceIDFieldInfo.GetValue(obj);
            return instanceID != 0;
        }
    }
}
