using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    [System.Serializable]
    public class Caster
    {
        internal delegate object CastFunc(object src, ICastSettings settings);

        internal class CastItem
        {
            public Type TargetType { get; set; }
            public CastFunc Cast { get; set; }
            public Type SettingsType { get; set; }

            public CastItem(Type targetType, CastFunc cast, Type settingsType = null)
            {
                TargetType = targetType;
                Cast = cast;
                SettingsType = settingsType;
            }
        }

        private static readonly Dictionary<Type, CastItem[]> SupportedCastMap = new Dictionary<Type, CastItem[]>
        {
            { 
                typeof(int), new CastItem[]
                {
                    new CastItem(typeof(float), (obj, s) => (float) obj),
                    new CastItem(typeof(byte), (obj, s) => (byte) obj),
                    new CastItem(typeof(short), (obj, s) => (short) obj),
                    new CastItem(typeof(ushort), (obj, s) => (ushort) obj),
                }
            },
            {
                typeof(float), new CastItem[]
                {
                    new CastItem(typeof(int), (obj, s) => ((FloatToIntCastSettings)s).Cast((float)obj), typeof(FloatToIntCastSettings)),
                }
            }
        };

        public static bool CanBeCasted(Type typeSrc, Type typeDst) => FindCaster(typeSrc, typeDst) != null;

        public static bool TryCast(object src, Type dstType, ICastSettings settings, out object dst)
        {
            CastItem ci = FindCaster(src.GetType(), dstType);
            if (ci != null)
            {
                dst = ci.Cast(src, settings);
                return true;
            }
            dst = null;
            return false;
        }

        public static Type GetCastSettingsType(Type src, Type dst)
        {
            CastItem ci = FindCaster(src, dst);
            return ci?.SettingsType;
        }

        public static bool HasCastSettings(Type src, Type dst) => (GetCastSettingsType(src, dst) != null);

        internal static CastItem FindCaster(Type typeSrc, Type typeDst)
        {
            if (SupportedCastMap.TryGetValue(typeSrc, out CastItem[] castTypes))
            {
                for (int i = 0; i < castTypes.Length; ++i)
                {
                    if (castTypes[i].TargetType == typeDst)
                    {
                        return castTypes[i];
                    }
                }
            }
            return null;
        }
    }
}
