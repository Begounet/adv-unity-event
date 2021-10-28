#if UNITY_EDITOR
using UnityEngine.Events;
using System.Linq;
using UnityEditor;

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
            string aueSettingsPath = AssetDatabase.FindAssets($"t:{nameof(AUESettings)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(aueSettingsPath))
            {
                UnityEngine.Debug.LogError($"Could not find settings of type {nameof(AUESettings)} in the project. Abort upgrade.");
                return;
            }

            var settings = AssetDatabase.LoadAssetAtPath<AUESettings>(aueSettingsPath);

            var allInstances = AssetDatabase.FindAssets("t:scene", settings.SceneDirectories)
                .Concat(AssetDatabase.FindAssets("t:prefab", settings.PrefabDirectories))
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
#endif