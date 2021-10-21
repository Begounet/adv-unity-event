using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace AUE
{
    public class AUELinkBuildProcessor : IUnityLinkerProcessor, IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public string LinkFilePath => UnityEngine.Application.dataPath + "/../Library/AUE.Generated/link.xml";

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            var registeredTypes = GetAllAUERegistered();

            string xmlContent = BuildLinkXmlFileContent(registeredTypes);

            string outputDir = Path.GetDirectoryName(LinkFilePath);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            File.WriteAllText(LinkFilePath, xmlContent);

            return LinkFilePath;
        }

        private static string BuildLinkXmlFileContent(Dictionary<Assembly, Dictionary<Type, HashSet<MethodInfo>>> registeredTypes)
        {
            StringBuilder sb = new StringBuilder();
            int indentLevel = 0;
            sb.AppendLine("<linker>");
            foreach (var kv in registeredTypes)
            {
                ++indentLevel;
                sb.Append(new string(' ', indentLevel));
                sb.AppendLine($"<assembly fullname=\"{kv.Key}\">");
                foreach (var methodPerType in kv.Value)
                {
                    ++indentLevel;
                    sb.Append(new string(' ', indentLevel));
                    sb.AppendLine($"<type fullname=\"{methodPerType.Key.FullName}\">");
                    foreach (var mi in methodPerType.Value)
                    {
                        ++indentLevel;
                        sb.Append(new string(' ', indentLevel));
                        sb.AppendLine($"<method name=\"{mi.Name}\" />");
                        --indentLevel;
                    }
                    sb.Append(new string(' ', indentLevel));
                    sb.AppendLine($"</type>");
                    --indentLevel;
                }
                sb.Append(new string(' ', indentLevel));
                sb.AppendLine($"</assembly>");
                --indentLevel;
            }
            sb.Append("</linker>");
            return sb.ToString();
        }

        private static Dictionary<Assembly, Dictionary<Type, HashSet<MethodInfo>>> GetAllAUERegistered()
        {
            var registeredTypes = new Dictionary<Assembly, Dictionary<Type, HashSet<MethodInfo>>>();
            foreach (MethodInfo mi in AUESimpleMethod.RegisteredMethods)
            {
                var reflectedType = mi.ReflectedType;
                if (registeredTypes.TryGetValue(reflectedType.Assembly, out Dictionary<Type, HashSet<MethodInfo>> typesList))
                {
                    if (!typesList.TryGetValue(reflectedType, out HashSet<MethodInfo> methods))
                    {
                        methods = new HashSet<MethodInfo>();
                        typesList.Add(reflectedType, methods);
                    }

                    if (!methods.Contains(mi))
                    {
                        methods.Add(mi);
                    }
                }
                else
                {
                    typesList = new Dictionary<Type, HashSet<MethodInfo>>();
                    HashSet<MethodInfo> miHash = new HashSet<MethodInfo>();
                    miHash.Add(mi);
                    typesList.Add(reflectedType, miHash);
                    registeredTypes.Add(reflectedType.Assembly, typesList);
                }
            }

            return registeredTypes;
        }

        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {

        }

        public void OnPreprocessBuild(BuildReport report)
        {
            AUESimpleMethod.RegisteredMethods.Clear();
            AUESimpleMethod.IsRegisteringMethods = true;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            AUESimpleMethod.IsRegisteringMethods = false;
        }
    }
}