using UnityEngine.Events;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AUE
{
    public static class Upgrader
    {
        /// <summary>
        /// Force reserialization of all scenes and prefabs in project
        /// </summary>
        [MenuItem("Tools/AdvUnityEvent/Upgrade")]
        public static void Upgrade()
        {
            var allInstances = AssetDatabase.FindAssets("t:scene")
                .Concat(AssetDatabase.FindAssets("t:prefab"))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where((path) => path.StartsWith("Assets/"))
                .ToArray();
            AssetDatabase.ForceReserializeAssets(allInstances, ForceReserializeAssetsOptions.ReserializeAssets);
            foreach (var instance in allInstances)
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(instance);
                EditorUtility.SetDirty(obj);
            }
            EditorApplication.ExecuteMenuItem("File/Save");
        }

        public static void ToAUEEvent(UnityEngine.Object owner, UnityEventBase uEvent, BaseAUEEvent aueEvent)
            => UnityEventsUpgrader.ToAUEEvent(owner, uEvent, aueEvent);
    }
}