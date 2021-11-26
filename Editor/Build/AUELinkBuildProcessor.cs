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
        private interface IRegisterableField
        {
            void AddIntoLinkXML(StringBuilder sb);
        }

        private class RegisterableMethod : IRegisterableField
        {
            public MethodInfo MethodInfo { get; set; }

            public RegisterableMethod(MethodInfo mi) => MethodInfo = mi;

            public void AddIntoLinkXML(StringBuilder sb) => sb.AppendLine($"<method name=\"{MethodInfo.Name}\" />");

            public override bool Equals(object obj)
            {
                if (obj is RegisterableMethod method)
                {
                    return (method.MethodInfo == MethodInfo);
                }
                if (obj is MethodInfo mi)
                {
                    return (mi == MethodInfo);
                }
                return false;
            }

            public override int GetHashCode() => MethodInfo.GetHashCode();
        }

        private class RegisterableMember : IRegisterableField
        {
            public MemberInfo MemberInfo { get; set; }

            public RegisterableMember(MemberInfo mi) => MemberInfo = mi;

            public void AddIntoLinkXML(StringBuilder sb)
            {
                string typeName = string.Empty;
                switch (MemberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        typeName = "field";
                        break;
                    case MemberTypes.Property:
                        typeName = "property";
                        break;
                    case MemberTypes.Event:
                        typeName = "event";
                        break;
                }
                if (string.IsNullOrEmpty(typeName))
                {
                    Debug.LogError($"Could not register member '{MemberInfo.DeclaringType}.{MemberInfo.Name}' into AOT link.xml. " +
                        $"Unsupported member type ({MemberInfo.MemberType}).");
                    return;
                }

                sb.AppendLine($"<{typeName} name=\"{MemberInfo.Name}\" />");
            }

            public override bool Equals(object obj)
            {
                if (obj is RegisterableMember member)
                {
                    return (member.MemberInfo == MemberInfo);
                }
                else if (obj is MemberInfo mi)
                {
                    return (mi == MemberInfo);
                }
                return false;
            }

            public override int GetHashCode() => MemberInfo.GetHashCode();
        }


        public int callbackOrder => 0;

        public string LinkFilePath => Application.dataPath + "/../Library/AUE.Generated/link.xml";

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

        private static string BuildLinkXmlFileContent(Dictionary<Assembly, Dictionary<Type, HashSet<IRegisterableField>>> registeredFields)
        {
            StringBuilder sb = new StringBuilder();
            int indentLevel = 0;
            sb.AppendLine("<linker>");
            foreach (var kv in registeredFields)
            {
                ++indentLevel;
                sb.Append(new string('\t', indentLevel));
                sb.AppendLine($"<assembly fullname=\"{kv.Key}\">");
                foreach (var fieldPerType in kv.Value)
                {
                    ++indentLevel;
                    sb.Append(new string('\t', indentLevel));
                    sb.AppendLine($"<type fullname=\"{fieldPerType.Key.FullName}\">");
                    foreach (var field in fieldPerType.Value)
                    {
                        ++indentLevel;
                        sb.Append(new string('\t', indentLevel));
                        field.AddIntoLinkXML(sb);
                        --indentLevel;
                    }
                    sb.Append(new string('\t', indentLevel));
                    sb.AppendLine($"</type>");
                    --indentLevel;
                }
                sb.Append(new string('\t', indentLevel));
                sb.AppendLine($"</assembly>");
                --indentLevel;
            }
            sb.Append("</linker>");
            return sb.ToString();
        }

        private static Dictionary<Assembly, Dictionary<Type, HashSet<IRegisterableField>>> GetAllAUERegistered()
        {
            var registeredTypes = new Dictionary<Assembly, Dictionary<Type, HashSet<IRegisterableField>>>();
            foreach (MethodInfo mi in AUESimpleMethod.RegisteredMethods)
            {
                var miField = new RegisterableMethod(mi);
                RegisterField(registeredTypes, mi.ReflectedType, miField);
            }
            foreach (MemberInfo mi in AUESimpleMethod.RegisteredMembers)
            {
                var miField = new RegisterableMember(mi);
                RegisterField(registeredTypes, mi.ReflectedType, miField);
            }
            return registeredTypes;
        }

        private static void RegisterField(Dictionary<Assembly, Dictionary<Type, HashSet<IRegisterableField>>> registeredTypes, Type reflectedType, IRegisterableField miField)
        {
            if (registeredTypes.TryGetValue(reflectedType.Assembly, out Dictionary<Type, HashSet<IRegisterableField>> typesList))
            {
                if (!typesList.TryGetValue(reflectedType, out HashSet<IRegisterableField> fields))
                {
                    fields = new HashSet<IRegisterableField>();
                    typesList.Add(reflectedType, fields);
                }

                if (!fields.Contains(miField))
                {
                    fields.Add(miField);
                }
            }
            else
            {
                typesList = new Dictionary<Type, HashSet<IRegisterableField>>();
                var miHash = new HashSet<IRegisterableField>();
                miHash.Add(miField);
                typesList.Add(reflectedType, miHash);
                registeredTypes.Add(reflectedType.Assembly, typesList);
            }
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
            AUESimpleMethod.RegisteredMembers.Clear();
            AUESimpleMethod.IsRegisteringMethods = true;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            AUESimpleMethod.IsRegisteringMethods = false;

            // Release memory
            AUESimpleMethod.RegisteredMethods.Clear();
            AUESimpleMethod.RegisteredMembers.Clear();
        }
    }
}