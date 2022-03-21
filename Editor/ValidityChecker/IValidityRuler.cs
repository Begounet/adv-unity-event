using UnityEditor;
using UnityEngine;

namespace AUE
{
    public interface IValidityRuler
    {
        bool Check(SerializedProperty aueSP, VCContext ctx);
    }
}
