using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AUE
{
    public class ValidityChecker
    {
        private enum ERunMode
        {
            Stopped,
            Paused,
            Running,
        }

        private ERunMode _runMode = ERunMode.Stopped;

        private static readonly IValidityRuler[] Rules = new IValidityRuler[]
        {
            new CustomArgumentTypeSyncRuler()
        };

        [MenuItem("Tools/AdvUnityEvent/Check Validity")]
        public static void CheckValidity() => new ValidityChecker().Run();

        public void Run() => EditorCoroutineUtility.StartCoroutine(RunAsync(), this);

        private IEnumerator RunAsync()
        {
            _runMode = ERunMode.Running;

            int progressId = Progress.Start("Check validity", options: Progress.Options.Managed);
            RegisterProgressAction(progressId);
            {
                var ctx = new VCContext() { ProgressId = progressId };
                string[] targetGUIDs = GetTargetGUIDs();
                yield return CheckTargetsValidity(targetGUIDs, ctx);
            }
            UnregisterProgressAction(progressId);
            Progress.Finish(progressId);
        }

        private void RegisterProgressAction(int progressId)
        {
            Progress.RegisterPauseCallback(progressId, (requiresPause) =>
            {
                if (requiresPause)
                {
                    _runMode = ERunMode.Paused;
                }
                else
                {
                    _runMode = ERunMode.Running;
                }
                return true;
            });
            Progress.RegisterCancelCallback(progressId, () =>
            {
                _runMode = ERunMode.Stopped;
                return true;
            });
        }

        private void UnregisterProgressAction(int progressId)
        {
            Progress.UnregisterCancelCallback(progressId);
            Progress.UnregisterPauseCallback(progressId);
        }

        private IEnumerator CheckTargetsValidity(string[] targetGUIDs, VCContext ctx)
        {
            for (int i = 0; i < targetGUIDs.Length && _runMode != ERunMode.Stopped; ++i)
            {
                if (ctx.ProgressId >= 0)
                {
                    Progress.Report(ctx.ProgressId, i, targetGUIDs.Length);
                }

                string assetPath = AssetDatabase.GUIDToAssetPath(targetGUIDs[i]);
                ctx.PushStacktrace(assetPath);
                ctx.SetProgressionDescription(Path.GetFileName(assetPath));

                string ext = Path.GetExtension(assetPath);
                switch (ext)
                {
                    case ".asset":
                        var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                        yield return CheckScriptableObjectValidity(so, ctx);
                        break;
                    case ".unity":
                        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                        yield return CheckSceneValidity(scene, ctx);
                        break;
                    case ".prefab":
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        yield return CheckGameObjectTreeValidity(prefab, ctx);
                        break;
                }
                ctx.PopStackTrace();

                while (_runMode == ERunMode.Paused)
                {
                    yield return null;
                }
            }
        }

        private IEnumerator CheckScriptableObjectValidity(UnityEngine.Object obj, VCContext ctx)
        {
            yield return CheckSerializedObjectValidity(new SerializedObject(obj), ctx);
        }

        private IEnumerator CheckSceneValidity(SceneAsset scene, VCContext ctx)
        {
            string assetPath = AssetDatabase.GetAssetPath(scene);
            Scene loadedScene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Additive);

            ctx.PushStacktrace(scene.name);
            GameObject[] roots = loadedScene.GetRootGameObjects();
            for (int i = 0; i < roots.Length && _runMode == ERunMode.Running; ++i)
            {
                yield return CheckGameObjectTreeValidity(roots[i], ctx);

                while (_runMode == ERunMode.Paused)
                {
                    yield return null;
                }
            }

            ctx.PopStackTrace();

            EditorSceneManager.CloseScene(loadedScene, removeScene: true);
        }

        private IEnumerator CheckGameObjectTreeValidity(GameObject root, VCContext ctx)
        {
            ctx.PushStacktrace(root.name);
            yield return CheckGameObjectValidity(root, ctx);

            var trans = root.transform;
            for (int i = 0; i < trans.childCount && _runMode == ERunMode.Running; ++i)
            {
                var child = trans.GetChild(i).gameObject;
                yield return CheckGameObjectTreeValidity(child, ctx);

                while (_runMode == ERunMode.Paused)
                {
                    yield return null;
                }
            }
            ctx.PopStackTrace();
        }

        private IEnumerator CheckGameObjectValidity(GameObject go, VCContext ctx)
        {
            var mbs = go.GetComponents<MonoBehaviour>();
            for (int i = 0; i < mbs.Length && _runMode == ERunMode.Running; ++i)
            {
                var mb = mbs[i];
                if (mb == null)
                {
                    continue;
                }

                ctx.PushStacktrace(mb.name);
                yield return CheckSerializedObjectValidity(new SerializedObject(mb), ctx);
                ctx.PopStackTrace();

                while (_runMode == ERunMode.Paused)
                {
                    yield return null;
                }
            }
        }

        private static IEnumerator CheckSerializedObjectValidity(SerializedObject so, VCContext ctx)
        {
            var itSP = so.GetIterator();
            bool enterChildren = true;
            while (itSP.NextVisible(enterChildren))
            {
                enterChildren = true;
                if (itSP.propertyType == SerializedPropertyType.Generic &&
                    LooksLikeAUESimpleMethodLayout(itSP))
                {
                    CheckAUESimpleMethodValidity(itSP, ctx);
                    enterChildren = false; // No need to go deeper in the property
                }
                ctx.IncreaseFrameWorkItemCount();
                if (ctx.HasReachedYieldWork())
                {
                    ctx.ResetFrameWorkItemCount();
                    yield return null;
                }
            }
        }

        private static bool LooksLikeAUESimpleMethodLayout(SerializedProperty sp)
            => sp.HasProperty("_id")
            && sp.HasProperty("_identifier")
            && sp.HasProperty("_target")
            && sp.HasProperty("_methodName")
            && sp.HasProperty("_parameterInfos");

        private static void CheckAUESimpleMethodValidity(SerializedProperty aueSP, VCContext ctx)
        {
            for (int i = 0; i < Rules.Length; ++i)
            {
                if (!Rules[i].Check(aueSP, ctx))
                {
                    return;
                }
            }
        }

        private static string[] GetTargetGUIDs()
        {
            string[] targets = AssetDatabase.FindAssets("t:prefab");
            ArrayUtility.AddRange(ref targets, AssetDatabase.FindAssets("t:scene"));
            ArrayUtility.AddRange(ref targets, AssetDatabase.FindAssets("t:scriptableobject"));
            return targets;
        }
    }
}
