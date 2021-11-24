using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AUE
{
    public static class MemberInfoCache
    {
        private static Dictionary<Type, MemberInfo[]> MemberInfos = new Dictionary<Type, MemberInfo[]>();

        public static MemberInfo[] GetMemberInfos(Type t, BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            if (!MemberInfos.TryGetValue(t, out MemberInfo[] memberInfos))
            {
                memberInfos = t.GetMembers(bf);
                MemberInfos.Add(t, memberInfos);
            }
            return memberInfos;
        }
    }
}
