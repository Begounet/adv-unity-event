using UnityEngine;

namespace AUE
{
    public class AUESettings : ScriptableObject
    {
        public enum ENullTargetBehavior
        {
            Exception,
            Ignore,
        }

        [Header("Editor")]

        [SerializeField]
        [Tooltip("Type searchs for some editor fields will include only types inside this assembly")]
        private AssemblyReference[] _typesAssemblies = new AssemblyReference[] 
        {
            new AssemblyReference(typeof(string)),
            new AssemblyReference(typeof(UnityEngine.Object))
        };
        public AssemblyReference[] TypesAssemblies => _typesAssemblies;

        [SerializeField, Tooltip("Determines the behavior when a null target is found in an AUE Method")]
        private ENullTargetBehavior _nullTargetBehavior;
        public ENullTargetBehavior NullTargetBehavior => _nullTargetBehavior;

        [Header("Upgrader")]

        [SerializeField]
        [Tooltip("Included directories when upgrading scenes. If nothing, all scenes of the project will be upgraded.")]
        private string[] _sceneDirectories = new string[] { "Scenes" };
        public string[] SceneDirectories
        {
            get => _sceneDirectories;
            set => _sceneDirectories = value;
        }

        [SerializeField]
        [Tooltip("Included directories when upgrading prefabs. If nothing, all prefabs of the project will be upgraded.")]
        private string[] _prefabDirectories = new string[] { "Prefabs" };
        public string[] PrefabDirectories
        {
            get => _prefabDirectories;
            set => _prefabDirectories = value;
        }
    }
}