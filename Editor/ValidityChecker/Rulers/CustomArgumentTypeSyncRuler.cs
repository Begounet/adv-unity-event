using UnityEditor;
using UnityEngine;

namespace AUE
{
    public class CustomArgumentTypeSyncRuler : IValidityRuler
    {
        public bool Check(SerializedProperty aueSP, VCContext ctx)
        {
            var parameterInfosSP = aueSP.FindPropertyRelative(AUEUtils.ParameterInfosSPName);
            for (int i = 0; i < parameterInfosSP.arraySize; ++i)
            {
                var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(i);
                var modeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ModeSPName);
                var customArgumentSP = parameterInfoSP.FindPropertyRelative(AUEUtils.CustomArgumentSPName);

                var mode = (AUEMethodParameterInfo.EMode)modeSP.enumValueIndex;
                var customArgumentType = AUEMethodParameterInfoPropertyDrawer.GetArgumentTypeFromMode(mode);

                string customArgumentFullTypename = 
                    customArgumentSP.managedReferenceFullTypename.Remove(0, customArgumentSP.managedReferenceFullTypename.IndexOf(' ') + 1);

                if (customArgumentFullTypename != customArgumentType.FullName)
                {
                    ctx.LogError($"Mode and custom argument type conflict on parameter ! (Mode={mode}, Argument Type={customArgumentFullTypename})");
                }
            }
            return true;
        }
    }
}
