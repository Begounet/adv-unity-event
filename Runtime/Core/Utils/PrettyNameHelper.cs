using System;
using System.Reflection;
using System.Text;
using UnityEngine.Pool;

namespace AUE
{
    public static class PrettyNameHelper
    {
        public static string GeneratePrettyName(object target, MethodInfo mi)
        {
            StringBuilder sb = UnsafeGenericPool<StringBuilder>.Get();
            sb.Clear();
            {
                sb.Append(mi.ReturnType.Name);
                sb.Append(' ');
                sb.Append('(');
                if (target != null)
                {
                    sb.Append(target);
                }
                else
                {
                    sb.Append("<null>");
                }
                sb.Append(").");
                sb.Append(mi.Name);
                sb.Append('(');

                ParameterInfo[] pis = mi.GetParameters();
                for (int i = 0; i < pis.Length; ++i)
                {
                    var pi = pis[i];
                    sb.Append(pi.ParameterType.Name);
                    sb.Append(' ');
                    sb.Append(pi.Name);
                    if (i + 1 < pis.Length)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(')');
            }
            string result = sb.ToString();
            UnsafeGenericPool<StringBuilder>.Release(sb);
            return result;
        }

        public static string GeneratePrettyName(Delegate[] invokeList, int maxDisplayed = 4)
        {
            StringBuilder sb = UnsafeGenericPool<StringBuilder>.Get();
            sb.Clear();
            {
                for (int i = 0; i < invokeList.Length && i < maxDisplayed; ++i)
                {
                    string invokeItemPrettyName = GeneratePrettyName(invokeList[i]);
                    sb.Append(invokeItemPrettyName);
                    if (i + 1 < invokeList.Length)
                    {
                        sb.Append(" | ");
                    }
                    if (i + 1 == maxDisplayed)
                    {
                        sb.Append('…');
                    }
                }
            }
            string result = sb.ToString();
            UnsafeGenericPool<StringBuilder>.Release(sb);
            return result;
        }

        public static string GeneratePrettyName(Delegate dlg) => GeneratePrettyName(dlg.Target, dlg.Method);

        public static string GeneratePrettyName(BaseAUEEvent aueEvent, Delegate[] invokeList)
        {
            string prettyName = string.Empty;
            if (aueEvent.Events.Count == 0 && invokeList.Length == 0)
            {
                // Will return that there is no event
                return aueEvent.GeneratePrettyName();
            }
            if (aueEvent.Events.Count > 0)
            {
                prettyName = aueEvent.GeneratePrettyName();
            }
            if (invokeList.Length > 0)
            {
                string runtimeInvokeListPrettyName = GeneratePrettyName(invokeList);
                if (!string.IsNullOrEmpty(prettyName))
                {
                    prettyName = prettyName + " | " + runtimeInvokeListPrettyName;
                }
            }
            return prettyName;
        }
    }
}
