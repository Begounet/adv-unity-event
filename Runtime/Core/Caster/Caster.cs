using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    [Serializable]
    public class Caster
    {
        public delegate object CastFunc(object src, ICastSettings settings);
        public delegate TDst CastFunc<TSrc, TDst>(TSrc src, ICastSettings settings);
        public delegate TDst CastFunc<TSrc, TDst, TSettings>(TSrc src, TSettings settings);

        public class CastItem
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

        public class CastItem<TSrc, TDst> : CastItem
        {
            public CastItem(CastFunc<TSrc, TDst> typedCast, Type settingsType = null)
                : base(typeof(TDst), (obj, settings) => typedCast((TSrc)obj, settings), settingsType) { }
        }

        public class CastItem<TSrc, TDst, TSettings> : CastItem
        {
            public CastItem(CastFunc<TSrc, TDst, TSettings> typedCast)
                : base(typeof(TDst), (obj, settings) => typedCast((TSrc)obj, (TSettings)settings), typeof(TSettings)) { }
        }

        /// <remarks>
        /// Public so devs can add new custom casts.
        /// </remarks>
        /// <example>
        /// SupportedCastMap.Add(typeof(MyCustomClass), new CastItem[] { new CastItem<MyCustomClass, MyOtherClass>((obj, s) => ConvertMyCustomClassToMyOtherClass(obj); });
        /// </example>
        public static readonly Dictionary<Type, CastItem[]> SupportedCastMap = new Dictionary<Type, CastItem[]>
        {
            { 
                typeof(int), new CastItem[]
                {
                    new CastItem<int, float>((obj, s) => obj),
                    new CastItem<int, byte>((obj, s) => (byte) obj),
                    new CastItem<int, short>((obj, s) => (short) obj),
                    new CastItem<int, ushort>((obj, s) => (ushort) obj),
                }
            },
            {
                typeof(float), new CastItem[]
                {
                    new CastItem<float, int, FloatToIntCastSettings>((obj, s) => s.Cast(obj)),
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
