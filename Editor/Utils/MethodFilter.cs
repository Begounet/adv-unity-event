using System;
using System.Diagnostics;
using System.Reflection;

namespace AUE
{
    [DebuggerDisplay("{TargetType != null ? TargetType.Name : \"null\"} (RT = {ReturnType != null ? ReturnType.Name : \"null\"})")]
    public class MethodFilter
    {
        public Type TargetType { get; set; }
        public Type ReturnType { get; set; }
        public BindingFlags BindingFlags { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is MethodFilter mf)
            {
                return mf.TargetType == TargetType &&
                    mf.ReturnType == ReturnType &&
                    mf.BindingFlags == BindingFlags;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (TargetType != null ? TargetType.GetHashCode() : 1);
                hash = hash * 23 + (ReturnType != null ? ReturnType.GetHashCode() : 1);
                hash = hash * 23 + BindingFlags.GetHashCode();
                return hash;
            }
        }
    }
}