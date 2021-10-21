using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static AUE.AUEUtils;

namespace AUE
{
    /// <summary>
    /// Contains all methods of a target, according to its type.
    /// </summary>
    [DebuggerDisplay("{Target} (Methods Count = {Methods.Count})")]
    public class TargetInvokeInfo
    {
        public Object Target { get; set; }
        public List<MethodMetaData> Methods { get; set; } = new List<MethodMetaData>();
    }
}