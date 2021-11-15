using System;
using System.Text;

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
    }
}
