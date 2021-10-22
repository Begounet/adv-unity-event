using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    [CustomPropertyDrawer(typeof(AUESimpleMethod), useForChildren: true)]
    public class AUEMethodPropertyDrawer : PropertyDrawer
    {
        private class PropertyMetaData
        {
            public InvokeInfo NewInvokeInfo = null;
        }

        private const string MethodNameSPName = "_methodName";
        private const string TargetSPName = "_target";
        private const string ParameterInfosSPName = "_parameterInfos";
        private const string ParameterTypeSPName = "_parameterType";
        private const string CallStateSPName = "_callState";

        private Dictionary<string, PropertyMetaData> _metaData = new Dictionary<string, PropertyMetaData>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;

            var targetSP = property.FindPropertyRelative(TargetSPName);
            if (targetSP.objectReferenceValue != null)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var parameterInfosSP = property.FindPropertyRelative(ParameterInfosSPName);
                if (parameterInfosSP.arraySize > 0)
                {
                    // Foldout label
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (parameterInfosSP.isExpanded)
                    {
                        // Parameter infos heights
                        for (int i = 0; i < parameterInfosSP.arraySize; ++i)
                        {
                            height += EditorGUI.GetPropertyHeight(parameterInfosSP.GetArrayElementAtIndex(i));
                            height += EditorGUIUtility.standardVerticalSpacing;
                        }
                    }
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect lineRect = position;
            lineRect.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(lineRect, property.FindPropertyRelative(CallStateSPName));
            lineRect.y += lineRect.height + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginChangeCheck();
            var targetSP = property.FindPropertyRelative(TargetSPName);
            EditorGUI.PropertyField(lineRect, targetSP);
            lineRect.y += lineRect.height + EditorGUIUtility.standardVerticalSpacing;
            if (EditorGUI.EndChangeCheck())
            {
                UpdateMethodName(property, null);
                UpdateParameterInfos(property, null);
                AUEMethodDatabaseUtils.UpdateMethodDatabase(AUEUtils.FindAUERootInParent(property));
                SetMethodDirty(property);
            }

            if (targetSP.objectReferenceValue != null)
            {
                DrawMethodSelection(ref lineRect, targetSP.objectReferenceValue, property);

                ++EditorGUI.indentLevel;
                {
                    var parameterInfosSP = property.FindPropertyRelative(ParameterInfosSPName);
                    DrawParameterInfos(ref lineRect, property, parameterInfosSP);
                }
                --EditorGUI.indentLevel;
            }
        }

        private void DrawMethodSelection(ref Rect position, Object target, SerializedProperty property)
        {
            var methodNameSP = property.FindPropertyRelative(MethodNameSPName);
            if (AUEUtils.LoadMethodInfoFromAUEMethod(property, out TargetInvokeInfo[] invokeInfos, out InvokeInfo selectedInvoke))
            {
                string method = (selectedInvoke != null ? selectedInvoke.MethodMeta.DisplayName : "<None>");
                if (GUI.Button(position, new GUIContent(method)))
                {
                    var dropdown = new MethodSearchDropdown(property, invokeInfos,
                        (prop, newInvokeInfo) =>
                        {
                            // Defer new method assignation because we are not *inside* the property drawing
                            // when exiting the popup
                            var metaData = CreateTempMetaData(prop);
                            metaData.NewInvokeInfo = newInvokeInfo;
                        });
                    dropdown.Show(position);
                }

                var metaData = TryGetTempMetaData(property);
                if (metaData != null)
                {
                    DeleteTempMetaData(property);
                    var newInvokeInfo = metaData.NewInvokeInfo;
                    if (newInvokeInfo != selectedInvoke)
                    {
                        if (newInvokeInfo != null)
                        {
                            var methodMetaData = newInvokeInfo.MethodMeta;
                            UpdateMethodName(property, methodMetaData);
                            UpdateTarget(property, newInvokeInfo.Target);
                            UpdateParameterInfos(property, methodMetaData.MethodInfo);
                        }
                        else
                        {
                            UpdateMethodName(property, null);
                            UpdateParameterInfos(property, null);
                        }
                        AUEMethodDatabaseUtils.UpdateMethodDatabase(AUEUtils.FindAUERootInParent(property));
                        SetMethodDirty(property);

                        GUI.changed = true;
                    }
                }

                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private static void UpdateMethodName(SerializedProperty aueSP, AUEUtils.MethodMetaData methodMetaData)
        {
            var methodNameSP = aueSP.FindPropertyRelative(MethodNameSPName);
            methodNameSP.stringValue = (methodMetaData != null ? methodMetaData.MethodInfo.Name : null);
        }

        private void UpdateTarget(SerializedProperty aueSP, Object target)
        {
            var targetSP = aueSP.FindPropertyRelative(TargetSPName);
            targetSP.objectReferenceValue = target;
        }

        private void DrawParameterInfos(ref Rect position, SerializedProperty aueMethodSP, SerializedProperty parameterInfosSP)
        {
            if (parameterInfosSP.arraySize == 0)
            {
                return;
            }

            parameterInfosSP.isExpanded = EditorGUI.Foldout(position, parameterInfosSP.isExpanded, parameterInfosSP.displayName);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            if (parameterInfosSP.isExpanded)
            {
                ParameterInfo[] parameterInfos = AUEUtils.LoadParameterTypesFromAUEMethod(aueMethodSP);
                if (parameterInfos != null)
                {
                    ++EditorGUI.indentLevel;
                    for (int i = 0; i < parameterInfosSP.arraySize; ++i)
                    {
                        var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(i);
                        float height = EditorGUI.GetPropertyHeight(parameterInfoSP);
                        Rect propRect = new Rect(position.x, position.y, position.width, height);
                        GUI.Box(propRect, GUIContent.none);
                        EditorGUI.PropertyField(propRect, 
                            parameterInfoSP, 
                            new GUIContent($"{AUEUtils.MakeHumanDisplayType(parameterInfos[i].ParameterType)} {parameterInfos[i].Name}"),
                            parameterInfosSP.isExpanded);
                        position.y += height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    --EditorGUI.indentLevel;
                }
            }
        }

        private void UpdateParameterInfos(SerializedProperty aueSP, MethodInfo methodInfo)
        {
            var parameterInfosSP = aueSP.FindPropertyRelative(ParameterInfosSPName);
            if (methodInfo == null)
            {
                parameterInfosSP.arraySize = 0;
                return;
            }

            ParameterInfo[] pis = methodInfo.GetParameters();

            parameterInfosSP.arraySize = pis.Length;
            for (int i = 0; i < pis.Length; ++i)
            {
                // Initialize new parameter info
                var parameterInfoSP = parameterInfosSP.GetArrayElementAtIndex(i);
                InitializeParameterInfo(parameterInfoSP, pis[i].ParameterType);
            }

            // By default, expand the parameter when they have been updated
            parameterInfosSP.isExpanded = true;
        }

        private static void InitializeParameterInfo(SerializedProperty parameterInfoSP, System.Type parameterType)
        {
            var parameterTypeSP = parameterInfoSP.FindPropertyRelative(ParameterTypeSPName);
            SerializableTypeHelper.SetTypeName(parameterTypeSP, parameterType);

            var modeSP = parameterInfoSP.FindPropertyRelative(AUEUtils.ModeSPName);
            modeSP.enumValueIndex = (int)AUEMethodParameterInfo.EMode.Constant;

            var customArgumentSP = parameterInfoSP.FindPropertyRelative(AUEUtils.CustomArgumentSPName);
            customArgumentSP.managedReferenceValue = null;
        }

        private void SetMethodDirty(SerializedProperty property)
        {
            var method = property.GetTarget<AUESimpleMethod>();
            if (method != null)
            {
                method.SetDirty();
            }
        }

        private PropertyMetaData TryGetTempMetaData(SerializedProperty property)
        {
            if (_metaData.TryGetValue(property.propertyPath, out PropertyMetaData metaData))
            {
                return metaData;
            }
            return null;
        }

        private PropertyMetaData CreateTempMetaData(SerializedProperty property)
        {
            var metaData = new PropertyMetaData();
            _metaData.Add(property.propertyPath, metaData);
            return metaData;
        }

        private void DeleteTempMetaData(SerializedProperty property)
        {
            _metaData.Remove(property.propertyPath);
        }
    }
}