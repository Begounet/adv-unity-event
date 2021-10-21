using System;
using UnityEditor;

namespace AUE
{
	public class SerializableTypeHelper
    {
        public const string SerializedTypeFullNameSPName = "_typeFullname";

        public static string GetTypeName(SerializedProperty serializableTypeSP)
        {
            var prop = serializableTypeSP.FindPropertyRelative(SerializedTypeFullNameSPName);
            return prop.stringValue;
        }

        public static void SetTypeName(SerializedProperty serializableTypeSP, string typeName)
        {
            var prop = serializableTypeSP.FindPropertyRelative(SerializedTypeFullNameSPName);
            prop.stringValue = typeName;
        }

        public static void SetTypeName(SerializedProperty serializableTypeSP, Type type)
            => SetTypeName(serializableTypeSP, GetTypeName(type));

        public static Type LoadType(SerializedProperty serializableTypeSP)
            => Type.GetType(GetTypeName(serializableTypeSP));

        public static void CopySerializableType(SerializedProperty src, SerializedProperty dst)
        {
            SetTypeName(dst, GetTypeName(src));
        }

        public static string GetTypeName(Type type) => type.AssemblyQualifiedName;
    }
}