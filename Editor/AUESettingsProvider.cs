using System;
using UnityEngine;
using UnityEditor;

namespace AUE
{
    public class AUESettingsProvider
    {
        private const string ProjectTitlePath = "Project/Adv Unity Event";
        private const string DefaultAssetPath = "Assets/AdvUnityEvent Settings.asset";
        private static readonly string[] Keywords =
        {
            "advanced",
            "event",
            "adv",
            "aue"
        };

        private const string TypesAssembliesGUIDSPName = "_typesAssembliesGUID";

        private static SerializedObject _settingsSO = null;

        [SettingsProvider]
        private static SettingsProvider CreateProjectPreferencesSettingsProvider()
        {
            return new SettingsProvider(ProjectTitlePath, SettingsScope.Project)
            {
                guiHandler = DrawProjectGUI,
                keywords = Keywords
            };
        }

        private static void DrawProjectGUI(string obj)
        {
            if (_settingsSO == null)
            {
                var settings = GetOrCreateSettings<AUESettings>();
                _settingsSO = new SerializedObject(settings);
            }

            if (_settingsSO == null)
            {
                return;
            }

            _settingsSO.Update();
            DrawSettings();
        }

        private static void DrawSettings()
        {
            EditorGUI.BeginChangeCheck();

            SerializedProperty sp = _settingsSO.GetIterator();

            sp.NextVisible(true);
            while (sp.NextVisible(false))
            {
                EditorGUILayout.PropertyField(sp);
            }

            if (EditorGUI.EndChangeCheck())
            {
                _settingsSO.ApplyModifiedProperties();
            }
        }

        public static T GetOrCreateSettings<T>() where T : ScriptableObject
        {
            T settings = null;

            string[] settingsPaths = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (settingsPaths.Length == 0)
            {
                settings = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(settings, DefaultAssetPath);
            }
            else
            {
                if (settingsPaths.Length > 1)
                {
                    Debug.LogWarning($"Multiple {typeof(T).Name} have been found. Should be only one in the project.");
                }

                string settingsPath = AssetDatabase.GUIDToAssetPath(settingsPaths[0]);
                settings = AssetDatabase.LoadAssetAtPath<T>(settingsPath);
            }

            return settings;
        }

        public static T GetSettings<T>() where T : ScriptableObject
        {
            T settings = null;

            string[] settingsPaths = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (settingsPaths.Length == 0)
            {
                return settings;
            }

            if (settingsPaths.Length > 1)
            {
                Debug.LogWarning($"Multiple {typeof(T).Name} have been found. Should be only one in the project.");
            }

            string settingsPath = AssetDatabase.GUIDToAssetPath(settingsPaths[0]);
            settings = AssetDatabase.LoadAssetAtPath<T>(settingsPath);
            return settings;
        }
    }
}

