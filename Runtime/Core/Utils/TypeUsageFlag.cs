using System;
using UnityEngine;

namespace AUE
{
    [Flags]
    public enum ETypeUsageFlag
    {
        Class = 0x01,
        Struct = 0x02,
        Abstract = 0x04,
        Interface = 0x08
    }
}
