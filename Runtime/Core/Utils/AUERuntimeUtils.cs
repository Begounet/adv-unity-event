using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AUE
{
    public static class AUERuntimeUtils
    {
        private static readonly FieldInfo CachedPtrFI = typeof(UnityEngine.Object).GetField("m_CachedPtr", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsUnityObjectValid(UnityEngine.Object obj)
        {
            IntPtr instanceID = (IntPtr)CachedPtrFI.GetValue(obj);
            return instanceID != IntPtr.Zero;
        }
    }
}
