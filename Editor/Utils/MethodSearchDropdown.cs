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

        private class ItemMetaData
        {
            public TargetInvokeInfo TargetInvokeInfo { get; set; }
            public MethodMetaData MethodMetaData { get; set; }
        }

        private TargetInvokeInfo[] _targetInvokeInfos;
        private MethodSelectionDelegate _methodSelectedIndexCallback;
        private SerializedProperty _property;

        private Dictionary<int, ItemMetaData> _itemMetaData;

        public MethodSearchDropdown(SerializedProperty property, TargetInvokeInfo[] invokeInfos, MethodSelectionDelegate methodSelectedIndexCallback)
             : base(new AdvancedDropdownState())
        {
            _property = property;
            _targetInvokeInfos = invokeInfos;
            _methodSelectedIndexCallback = methodSelectedIndexCallback;

            minimumSize = new Vector2(minimumSize.x, 400);

            _itemMetaData = new Dictionary<int, ItemMetaData>();
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item.id == -1 || !_itemMetaData.TryGetValue(item.id, out ItemMetaData selectedItem))
            {
                _methodSelectedIndexCallback.Invoke(_property, null);
                return;
            }

            _methodSelectedIndexCallback.Invoke(_property, new InvokeInfo()
            {
                Target = selectedItem.TargetInvokeInfo.Target,
                MethodMeta = selectedItem.MethodMetaData
            });
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("root");

            int metaIdx = 0;
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

                        AddMethodsToGroup(targetInvokeInfo, declaringTypeGroupItem, methodGroups, ref metaIdx);
                        if (methodsCount > 0 && settersCount > 0)
                        {
                            declaringTypeGroupItem.AddSeparator();
                            declaringTypeGroupItem.AddChild(new AdvancedDropdownItem("Setters") { enabled = false });
                            declaringTypeGroupItem.AddSeparator();
                        }
                        AddMethodsToGroup(targetInvokeInfo, declaringTypeGroupItem, setterGroups, ref metaIdx);
                        if ((methodsCount > 0 || settersCount > 0) && gettersCount > 0)
                        {
                            declaringTypeGroupItem.AddSeparator();
                            declaringTypeGroupItem.AddChild(new AdvancedDropdownItem("Getters") { enabled = false });
                            declaringTypeGroupItem.AddSeparator();
                        }
                        AddMethodsToGroup(targetInvokeInfo, declaringTypeGroupItem, getterGroups, ref metaIdx);
                    }

                    root.AddChild(targetInvokeInfoItem);
                }
            }

            root.AddSeparator();
            root.AddChild(new AdvancedDropdownItem("None") { id = -1 });

            return root;
        }

        private void AddMethodsToGroup(TargetInvokeInfo targetInvokeInfo, AdvancedDropdownItem groupItem, IEnumerable<MethodMetaData> methodGroups, ref int metaIdx)
        {
            foreach (var mmd in methodGroups)
            {
                groupItem.AddChild(new AdvancedDropdownItem(BuildMethodDisplayName(mmd)) { id = metaIdx });
                _itemMetaData.Add(metaIdx++, new ItemMetaData()
                {
                    TargetInvokeInfo = targetInvokeInfo,
                    MethodMetaData = mmd,
                });
            }
        }

        private string BuildMethodDisplayName(MethodMetaData mmd)
            => (mmd.MethodInfo.IsPublic ? "+" : "-") + " " + mmd.DisplayName;

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