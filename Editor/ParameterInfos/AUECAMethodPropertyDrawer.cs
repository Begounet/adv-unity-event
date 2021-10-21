using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUECAMethod))]
    public class AUECAMethodPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(GetMethodSerializedProperty(property));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, GetMethodSerializedProperty(property));
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