using System;
using System.Diagnostics;
using static AUE.AUEUtils;

namespace AUE
{
    /// <summary>
    /// Represent one callable method, with its target
    /// </summary>
    [DebuggerDisplay("{Target} : {MethodMeta}")]
    public class InvokeInfo : IEquatable<InvokeInfo>
	{
        public UnityEngine.Object Target { get; set; }
        public MethodMetaData MethodMeta { get; set; }

        public bool Equals(InvokeInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return (other.Target == Target &&
                other.MethodMeta == MethodMeta);
        }
    }
}