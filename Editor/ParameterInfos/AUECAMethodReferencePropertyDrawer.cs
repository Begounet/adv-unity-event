using System.Collections.Generic;
using System.Reflection;
using TypeCodebase;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECAMethodReference))]
    public class AUECAMethodReferencePropertyDrawer : PropertyDrawer
    {
        public static void Initialize(SerializedProperty property)
        {
            // Force get method to initialize the method
            GetMethodSerializedProperty(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var methodSP = GetMethodSerializedProperty(property);
            var propertyDrawer = methodSP.GetPropertyDrawer();
            return propertyDrawer.GetPropertyHeight(methodSP, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var methodSP = GetMethodSerializedProperty(property);

            // Find and use property drawer to draw because Unity sucks and fallback on default property drawer
            // for no reason.
            var propertyDrawer = methodSP.GetPropertyDrawer();
            propertyDrawer?.OnGUI(position, methodSP, label);
        }

        private static SerializedProperty GetMethodSerializedProperty(SerializedProperty property)
        {
            var methodIdSP = property.FindPropertyRelative(AUEUtils.CAMethodIdSPName);
            var aueRootSP = AUEUtils.FindAUERootInParent(property);
            var methodDatabaseSP = aueRootSP.FindPropertyRelative(AUEUtils.MethodDatabaseSPName);
            var methodId = (byte)methodIdSP.intValue;
            if (AUEMethodDatabaseUtils.CreateOrGetMethodFromDatabase(aueRootSP, methodDatabaseSP, ref methodId, out SerializedProperty methodSP))
            {
                // Bind the new method to the CA method
                methodIdSP.intValue = methodId;

                // Assign the parameter type to the return type of the method
                var parameterInfoSP = property.GetParent();
                var parameterTypeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName);
                var returnTypeSP = methodSP.FindPropertyRelative(AUEUtils.ReturnTypeSPName);
                SerializableTypeHelper.CopySerializableType(parameterTypeSP, returnTypeSP);
            }
            return methodSP;
        }
    }
}