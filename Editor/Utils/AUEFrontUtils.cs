using AUE.Descriptors;
using UnityEditor;

namespace AUE
{
    /// <summary>
    /// Helper class to setup AUE stuff more easily
    /// </summary>
    public static class AUEFrontUtils
    {
        /// <summary>
        /// Add a new event to an AUEEvent, configured from the <paramref name="methodDescriptor"/>
        /// </summary>
        public static void AddAUEEvent(SerializedProperty aueEventSP, AUEMethodDescriptor methodDescriptor)
        {
            var eventsSP = aueEventSP.FindPropertyRelative(AUEEventPropertyDrawer.EventsSPName);
            ++eventsSP.arraySize;
            var eventSP = eventsSP.GetArrayElementAtIndex(eventsSP.arraySize - 1);
            ConfigureAUEMethod(eventSP, methodDescriptor);
        }

        /// <summary>
        /// Allow to configure/setup an AUE Method serialized property from a <see cref="AUEMethodDescriptor"/>
        /// </summary>
        public static void ConfigureAUEMethod(SerializedProperty aueMethodSP, AUEMethodDescriptor methodDescriptor)
        {
            aueMethodSP.FindPropertyRelative(AUEUtils.TargetSPName).objectReferenceValue = methodDescriptor.Target;
            aueMethodSP.FindPropertyRelative(AUEUtils.CallStateSPName).enumValueIndex = (int)methodDescriptor.CallState;
            aueMethodSP.FindPropertyRelative(AUEUtils.MethodNameSPName).stringValue = methodDescriptor.MethodName;
            SerializableTypeHelper.SetType(aueMethodSP.FindPropertyRelative(AUEUtils.ReturnTypeSPName), methodDescriptor.ReturnType);

            // Argument types
            SerializedProperty argumentTypesSP = aueMethodSP.FindPropertyRelative(AUEUtils.ArgumentTypesSPName);
            argumentTypesSP.arraySize = methodDescriptor.ArgumentTypes.Length;
            for (int i = 0; i < argumentTypesSP.arraySize; ++i)
            {
                var argumentTypeSP = argumentTypesSP.GetArrayElementAtIndex(i);
                SerializableTypeHelper.SetType(argumentTypeSP, methodDescriptor.ArgumentTypes[i]);
            }

            // Parameter infos
            SerializedProperty parameterInfosSP = aueMethodSP.FindPropertyRelative(AUEUtils.ParameterInfosSPName);
            parameterInfosSP.arraySize = methodDescriptor.Parameters.Length;
            for (int i = 0; i < parameterInfosSP.arraySize; ++i)
            {
                var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(i);
                AUEParameterDescriptor paramDesc = methodDescriptor.Parameters[i];
                SerializableTypeHelper.SetType(parameterInfoSP.FindPropertyRelative(AUEUtils.ParameterInfoTypeSPName), paramDesc.ParameterType);
                parameterInfoSP.FindPropertyRelative(AUEUtils.ModeSPName).enumValueIndex = (int)paramDesc.Mode;
                parameterInfoSP.FindPropertyRelative(AUEUtils.CustomArgumentSPName).managedReferenceValue = paramDesc.CustomArgument;
            }
        }
    }
}
