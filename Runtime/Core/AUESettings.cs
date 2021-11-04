using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AUE
{
    public class AUESettings : ScriptableObject
    {
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