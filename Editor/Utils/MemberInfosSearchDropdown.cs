using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AUE
{
    public class MemberInfosSearchDropdown : AdvancedDropdown
    {
        public delegate void MemberInfoSelectionDelegate(SerializedProperty property, MemberInfo memberInfo, object userData);

        private MemberInfo[] _memberInfos;
        private SerializedProperty _property;
        private MemberInfoSelectionDelegate _onItemSelected;
        private object _userData;

        public MemberInfosSearchDropdown(SerializedProperty property, 
            Type type, object userData,
            MemberInfoSelectionDelegate onItemSelected,
            BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            : base(new AdvancedDropdownState())
        {
            _property = property;
            _onItemSelected = onItemSelected;
            _memberInfos = MemberInfoCache.GetMemberInfos(type, bf);
            _userData = userData;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_memberInfos[0].DeclaringType.Name);
            for (int i = 0; i < _memberInfos.Length; ++i)
            {
                var memberInfo = _memberInfos[i];
                
                Type memberType = null;
                if (memberInfo is PropertyInfo pi)
                {
                    memberType = pi.PropertyType;
                }
                else if (memberInfo is FieldInfo fi)
                {
                    memberType = fi.FieldType;
                }
                else
                {
                    continue;
                }

                var item = new AdvancedDropdownItem($"{AUEUtils.MakeHumanDisplayType(memberType)} {_memberInfos[i].Name}") { id = i };
                root.AddChild(item);
            }
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            var memberInfo = _memberInfos[item.id];
            _onItemSelected.Invoke(_property, memberInfo, _userData);
        }
    }
}
