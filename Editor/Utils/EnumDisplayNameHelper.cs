using System;
using System.Text;
using UnityEngine;

namespace AUE
{
    public static class EnumDisplayNameHelper
    {
        public static string GetName(Enum value, Type enumType)
        {
            int iValue = (int)(object)value;
            if (iValue == -1 || iValue == 0)
            {
                string name = Enum.GetName(enumType, value);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = iValue < 0 ? "Everything" : "Nothing";
                }
                return name;
            }

            bool isFlag = (enumType.GetCustomAttributes(typeof(FlagsAttribute), inherit: false).Length > 0);
            if (!isFlag)
            {
                return Enum.GetName(enumType, value);
            }

            Array values = Enum.GetValues(enumType);

            StringBuilder sb = new StringBuilder();
            foreach (var individualValue in values)
            {
                if ((int)individualValue != 0 && value.HasFlag((Enum)individualValue))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(',');
                    }
                    sb.Append(Enum.GetName(enumType, individualValue));
                }
            }
            return sb.ToString();
        }

        public static GUIContent[] BuildEnumOptions<T>()
        {
            Type type = typeof(T);
            Array enumValues = type.GetEnumValues();
            var options = new GUIContent[enumValues.Length];
            for (int i = 0; i < enumValues.Length; ++i)
            {
                T enumValue = (T)enumValues.GetValue(i);

                string enumName = enumValue.ToString();
                var attrs = 
                    type.GetField(enumName)
                    .GetCustomAttributes(typeof(EnumDescriptionAttribute), inherit: true) 
                    as EnumDescriptionAttribute[];

                string tooltip = string.Empty;
                if (attrs.Length > 0)
                {
                    tooltip = attrs[0].Description;
                }
                
                options[i] = new GUIContent(enumName, tooltip);
            }
            return options;
        }
    }
}
