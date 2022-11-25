using System;
using UnityEngine;

namespace AUE
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public EnumDescriptionAttribute(string desc)
        {
            Description = desc;
        }
    }
}
