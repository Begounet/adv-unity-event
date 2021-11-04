using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static AUE.AUEUtils;

namespace AUE
{
    public class MethodSearchDropdown : AdvancedDropdown
    {
        public delegate void MethodSelectionDelegate(SerializedProperty property, InvokeInfo selectedInvokeInfo);

        private TargetInvokeInfo[] _targetInvokeInfos;
        private MethodSelectionDelegate _methodSelectedIndexCallback;
        private SerializedProperty _property;

        public MethodSearchDropdown(SerializedProperty property, TargetInvokeInfo[] invokeInfos, MethodSelectionDelegate methodSelectedIndexCallback)
             : base(new AdvancedDropdownState())
        {
            _property = property;
            _targetInvokeInfos = invokeInfos;
            _methodSelectedIndexCallback = methodSelectedIndexCallback;

            minimumSize = new Vector2(minimumSize.x, 400);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item.id == -1)
            {
                _methodSelectedIndexCallback.Invoke(_property, null);
            }

            for (int i = 0; i < _targetInvokeInfos.Length; ++i)
            {
                var targetInvokeInfo = _targetInvokeInfos[i];
                for (int j = 0; j < targetInvokeInfo.Methods.Count; ++j)
                {
                    var methodMetaData = targetInvokeInfo.Methods[j];
                    if (methodMetaData.GetHashCode() == item.id)
                    {
                        _methodSelectedIndexCallback.Invoke(_property, new InvokeInfo()
                        {
                            Target = targetInvokeInfo.Target,
                            MethodMeta = methodMetaData
                        });
                    }
                }
            }
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("root");

            for (int i = 0; i < _targetInvokeInfos.Length; ++i)
            {
                var targetInvokeInfo = _targetInvokeInfos[i];
                if (targetInvokeInfo.Methods.Count > 0)
                {
                    var targetType = GetTargetType(targetInvokeInfo.Target);
                    var targetInvokeInfoItem = new AdvancedDropdownItem(MakeHumanDisplayType(targetType));

                    var groupQuery = targetInvokeInfo.Methods.GroupBy((mmd) => mmd.MethodInfo.DeclaringType)
                        .OrderBy((group) => GetDeclaringTypeDepth(targetType, group.Key));

                    foreach (var group in groupQuery)
                    {
                        var declaringTypeGroupItem = new AdvancedDropdownItem(MakeHumanDisplayType(group.Key));
                        targetInvokeInfoItem.AddChild(declaringTypeGroupItem);

                        var methodGroups = targetInvokeInfo.Methods.Where((mmd) => mmd.MethodInfo.DeclaringType == group.Key && IsMethod(mmd.MethodInfo));
                        var getterGroups = targetInvokeInfo.Methods.Where((mmd) => mmd.MethodInfo.DeclaringType == group.Key && IsGetter(mmd.MethodInfo));
                        var setterGroups = targetInvokeInfo.Methods.Where((mmd) => mmd.MethodInfo.DeclaringType == group.Key && IsSetter(mmd.MethodInfo));

                        int methodsCount = methodGroups.Count();
                        int gettersCount = getterGroups.Count();
                        int settersCount = setterGroups.Count();

                        if (methodsCount > 0 && (gettersCount > 0 || settersCount > 0))
                        {
                            declaringTypeGroupItem.AddChild(new AdvancedDropdownItem("Methods") { enabled = false });
                            declaringTypeGroupItem.AddSeparator();
                        }

                        AddMethodsToGroup(declaringTypeGroupItem, methodGroups);
                        if (methodsCount > 0 && settersCount > 0)
                        {
                            declaringTypeGroupItem.AddSeparator();
                            declaringTypeGroupItem.AddChild(new AdvancedDropdownItem("Setters") { enabled = false });
                            declaringTypeGroupItem.AddSeparator();
                        }
                        AddMethodsToGroup(declaringTypeGroupItem, setterGroups);
                        if ((methodsCount > 0 || settersCount > 0) && gettersCount > 0)
                        {
                            declaringTypeGroupItem.AddSeparator();
                            declaringTypeGroupItem.AddChild(new AdvancedDropdownItem("Getters") { enabled = false });
                            declaringTypeGroupItem.AddSeparator();
                        }
                        AddMethodsToGroup(declaringTypeGroupItem, getterGroups);
                    }

                    root.AddChild(targetInvokeInfoItem);
                }
            }

            root.AddSeparator();
            root.AddChild(new AdvancedDropdownItem("None") { id = -1 });

            return root;
        }

        private void AddMethodsToGroup(AdvancedDropdownItem groupItem, IEnumerable<MethodMetaData> methodGroups)
        {
            foreach (var mmd in methodGroups)
            {
                groupItem.AddChild(new AdvancedDropdownItem(mmd.DisplayName)
                {
                    id = mmd.GetHashCode()
                });
            }
        }

        private int GetDeclaringTypeDepth(Type srcType, Type declaringType)
        {
            if (srcType == declaringType)
            {
                return 0;
            }
            return GetDeclaringTypeDepth(srcType.BaseType, declaringType) + 1;
        }
    }
}