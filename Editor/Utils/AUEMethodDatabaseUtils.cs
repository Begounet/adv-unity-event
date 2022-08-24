using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using static AUE.AUEMethodParameterInfo;

namespace AUE
{
    public static class AUEMethodDatabaseUtils
    {
        public static void AddNewEntry(SerializedProperty aueSP, out SerializedProperty newEntrySP, out byte entryId)
        {
            var methodDatabaseSP = aueSP.FindPropertyRelative(AUEUtils.MethodDatabaseSPName);
            int availableId = FindAvailableId(methodDatabaseSP);
            int newIndex = methodDatabaseSP.arraySize;
            methodDatabaseSP.InsertArrayElementAtIndex(newIndex);
            newEntrySP = methodDatabaseSP.GetArrayElementAtIndex(newIndex);
            var idSP = newEntrySP.FindPropertyRelative(AUEUtils.IdSPName);
            idSP.intValue = availableId;
            entryId = (byte)idSP.intValue;
        }

        private static byte FindAvailableId(SerializedProperty methodDatabaseSP)
        {
            // Starts from 1. 0 is reserved by default, newly created method.
            byte availableId = 1;
            bool shouldContinue = true;
            while (shouldContinue)
            {
                shouldContinue = false;
                for (int i = 0; i < methodDatabaseSP.arraySize; ++i)
                {
                    var methodSP = methodDatabaseSP.GetArrayElementAtIndex(i);
                    var methodIdSP = methodSP.FindPropertyRelative(AUEUtils.IdSPName);
                    if (methodIdSP.intValue == availableId)
                    {
                        shouldContinue = true;
                        ++availableId;
                    }
                }
            }
            return availableId;
        }

        public static void DeleteEntry(SerializedProperty aueSP, byte id)
        {
            var methodDatabaseSP = aueSP.FindPropertyRelative(AUEUtils.MethodDatabaseSPName);
            int methodDatabaseIndex = FindMethodIndexById(methodDatabaseSP, id);
            if (methodDatabaseIndex >= 0)
            {
                methodDatabaseSP.DeleteArrayElementAtIndex(methodDatabaseIndex);
                UpdateMethodDatabase(aueSP);
            }
        }

        private static void RegisterUsedMethodId(SerializedProperty methodDatabaseSP, SerializedProperty aueMethodSP, List<(byte id, SerializedProperty sp)> ids)
        {
            var parameterInfosSP = aueMethodSP.FindPropertyRelative(AUEUtils.ParameterInfosSPName);
            for (int i = 0; i < parameterInfosSP.arraySize; ++i)
            {
                var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(i);
                var modeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ModeSPName);
                if ((EMode)modeSP.enumValueIndex == EMode.Method)
                {
                    var customArgumentSP = parameterInfoSP.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
                    byte methodId = (byte)customArgumentSP.FindPropertyRelative(AUEUtils.MethodIdSPName).intValue;

                    ids.Add((methodId, aueMethodSP));

                    SerializedProperty methodSP = FindMethodById(methodDatabaseSP, methodId);
                    if (methodSP != null)
                    {
                        RegisterUsedMethodId(methodDatabaseSP, methodSP, ids);
                    }
                }
            }
        }

        public static void UpdateMethodDatabase(SerializedProperty aueSP)
        {
            // Find all used methods Id from the database
            var usedIdList = new List<(byte id, SerializedProperty sp)>();
            var methodDatabaseSP = aueSP.FindPropertyRelative(AUEUtils.MethodDatabaseSPName);
            RegisterUsedMethodId(methodDatabaseSP, aueSP, usedIdList);
            RemoveOrphans(methodDatabaseSP, usedIdList);
            UpdateMethodIndexes(methodDatabaseSP, aueSP);
        }

        private static void RemoveOrphans(SerializedProperty methodDatabaseSP, List<(byte id, SerializedProperty sp)> usedIds)
        {
            for (int i = 0; i < methodDatabaseSP.arraySize; ++i)
            {
                var methodSP = methodDatabaseSP.GetArrayElementAtIndex(i);
                var methodIdSP = methodSP.FindPropertyRelative(AUEUtils.IdSPName);
                byte methodId = (byte)methodIdSP.intValue;

                var result = usedIds.FirstOrDefault((usedId) => usedId.id == methodId);
                if (result == default)
                {
                    methodDatabaseSP.DeleteArrayElementAtIndex(i);
                }
            }
        }

        // Update _methodIndex for fast runtime access
        private static void UpdateMethodIndexes(SerializedProperty methodDatabaseSP, SerializedProperty aueMethodSP)
        {
            BrowseCustomArguments(aueMethodSP, (customArgSP) =>
            {
                var methodIdSP = customArgSP.FindPropertyRelative(AUEUtils.MethodIdSPName);
                byte methodId = (byte)methodIdSP.intValue;
                int methodDatabaseIndex = FindMethodIndexById(methodDatabaseSP, methodId);
                var methodIndexSP = customArgSP.FindPropertyRelative(AUEUtils.MethodIndexSPName);
                methodIndexSP.intValue = methodDatabaseIndex;
            });
        }

        private static void BrowseCustomArguments(SerializedProperty aueMethodSP, Action<SerializedProperty> customArgumentCallback)
        {
            var parameterInfosSP = aueMethodSP.FindPropertyRelative(AUEUtils.ParameterInfosSPName);
            for (int i = 0; i < parameterInfosSP.arraySize; ++i)
            {
                var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(i);
                var modeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ModeSPName);
                if ((EMode)modeSP.enumValueIndex == EMode.Method)
                {
                    var customArgumentSP = parameterInfoSP.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
                    customArgumentCallback.Invoke(customArgumentSP);
                }
            }
        }

        public static int FindMethodIndexById(SerializedProperty methodDatabaseSP, byte id)
        {
            for (int i = 0; i < methodDatabaseSP.arraySize; ++i)
            {
                var methodSP = methodDatabaseSP.GetArrayElementAtIndex(i);
                var methodIdSP = methodSP.FindPropertyRelative(AUEUtils.IdSPName);
                if ((byte)methodIdSP.intValue == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public static SerializedProperty FindMethodById(SerializedProperty methodDatabaseSP, byte id)
        {
            int index = FindMethodIndexById(methodDatabaseSP, id);
            if (index >= 0)
            {
                return methodDatabaseSP.GetArrayElementAtIndex(index);
            }
            return null;
        }

        /// <returns>True if a new entry is created</returns>
        public static bool CreateOrGetMethodFromDatabase(SerializedProperty aueSP, SerializedProperty methodDatabaseSP, ref byte id, out SerializedProperty methodSP)
        {
            methodSP = FindMethodById(methodDatabaseSP, id);
            if (methodSP == null)
            {
                AddNewEntry(aueSP, out methodSP, out id);
                InitializeNewEntry(methodSP);
                return true;
            }
            return false;
        }

        private static void InitializeNewEntry(SerializedProperty methodSP)
        {
            methodSP.FindPropertyRelative(AUEUtils.TargetSPName).objectReferenceValue = null;
            methodSP.FindPropertyRelative(AUEUtils.MethodNameSPName).stringValue = string.Empty;
            SerializableTypeHelper.SetType(methodSP.FindPropertyRelative(AUEUtils.ReturnTypeSPName), typeof(void));
            methodSP.FindPropertyRelative(AUEUtils.ParameterInfosSPName).arraySize = 0;
            methodSP.FindPropertyRelative(AUEUtils.BindingFlagsSPName).intValue = (int)
                (BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance
                | BindingFlags.Static
                | BindingFlags.GetField
                | BindingFlags.GetProperty);
        }
    }
}