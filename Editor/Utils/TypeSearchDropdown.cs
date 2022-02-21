using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AUE
{
    public class TypeSearchDropdown : AdvancedDropdown
    {
        public class Settings
        {
            public Type ConstraintType { get; set; }
            public ETypeUsageFlag UsageFlags { get; set; }
        }

        private const int NumOperationsAllowedPerFrame = 1000;
        private static Dictionary<Assembly, Type[]> _typesCache = null;
        private static FieldInfo _getWindowInstanceFI = null;

        private Settings _customSettings = null;
        private EditorCoroutine _currentBuildRootCoroutine = null;
        private int _numOperationsDone;

        public event Action<Type> OnTypeSelected;
        private EditorWindow _currentWindow;

        public TypeSearchDropdown(AdvancedDropdownState state, Settings customSettings = null)
            : base(state)
        {
            minimumSize = new Vector2(200, 300);
            _customSettings = customSettings;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Type");

            if (_currentBuildRootCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_currentBuildRootCoroutine);
            }

            _currentBuildRootCoroutine = EditorCoroutineUtility.StartCoroutine(BuildRootAsync(root), this);
            return root;
        }

        private IEnumerator BuildRootAsync(AdvancedDropdownItem root)
        {
            _currentWindow = GetCurrentWindow();
            _numOperationsDone = 0;
            _typesCache = new Dictionary<Assembly, Type[]>();
            var aueSettings = AUESettingsProvider.GetOrCreateSettings<AUESettings>();
            foreach (var assemblyRef in aueSettings.TypesAssemblies)
            {
                yield return BuildTypesForAssembly(root, assemblyRef);
            }
            root.AddSeparator();
            root.AddChild(new AdvancedDropdownItem("Undefined") { id = 0 });

            _currentWindow.Repaint();

            _currentBuildRootCoroutine = null;
        }

        private IEnumerator BuildTypesForAssembly(AdvancedDropdownItem root, AssemblyReference assemblyRef)
        {
            var assemblyNode = new AdvancedDropdownItem(assemblyRef.AssemblyShortName) { id = assemblyRef.GetHashCode() };

            if (_typesCache.TryGetValue(assemblyRef.Assembly, out Type[] types))
            {
                if (types.Length == 0)
                {
                    yield break;
                }

                for (int i = 0; i < types.Length; ++i)
                {
                    assemblyNode.AddChild(new AdvancedDropdownItem(types[i].Name) { id = types[i].GetHashCode() });
                    ++_numOperationsDone;

                    if (HasDoneEnoughOperationsThisFrame())
                    {
                        _currentWindow.Repaint();
                        yield return null;
                    }
                }
                root.AddChild(assemblyNode);
            }
            else
            {
                IEnumerable<Type> enumTypes;
                if (_customSettings != null)
                {
                    enumTypes = GetConstraintTypesEnumeratorFromAssembly(assemblyRef.Assembly, _customSettings);
                }
                else
                {
                    enumTypes = assemblyRef.Assembly.GetTypes();
                }

                foreach (var type in enumTypes)
                {
                    assemblyNode.AddChild(new AdvancedDropdownItem(type.Name) { id = type.GetHashCode() });
                    ++_numOperationsDone;

                    if (HasDoneEnoughOperationsThisFrame())
                    {
                        _currentWindow.Repaint();
                        yield return null;
                    }
                }

                // Check if there is at least one child
                if (assemblyNode.children.Any())
                {
                    root.AddChild(assemblyNode);
                }

                _typesCache.Add(assemblyRef.Assembly, enumTypes.ToArray());
            }
        }

        private bool HasDoneEnoughOperationsThisFrame()
            => (_numOperationsDone > 0 && _numOperationsDone % NumOperationsAllowedPerFrame == 0);

        private IEnumerable<Type> GetConstraintTypesEnumeratorFromAssembly(Assembly assembly, Settings settings = null)
        {
            Type[] types = assembly.GetTypes();
            if (settings == null)
            {
                return types;
            }

            return types
                .Where((t)
                => (settings.ConstraintType == null || settings.ConstraintType.IsAssignableFrom(t))
                && (!t.IsAbstract || settings.UsageFlags.HasFlag(ETypeUsageFlag.Abstract))
                && (!t.IsInterface || settings.UsageFlags.HasFlag(ETypeUsageFlag.Interface))
                && (!t.IsClass || settings.UsageFlags.HasFlag(ETypeUsageFlag.Class))
                && (!t.IsValueType || settings.UsageFlags.HasFlag(ETypeUsageFlag.Struct)));
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            Type selectedType = FindItemById(item.id);
            OnTypeSelected.Invoke(selectedType);
        }

        private Type FindItemById(int id)
        {
            if (id == 0)
            {
                return null;
            }
            foreach (var kv in _typesCache)
            {
                int count = kv.Value.Length;
                for (int i = 0; i < count; ++i)
                {
                    var type = kv.Value[i];
                    if (type.GetHashCode() == id)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        private EditorWindow GetCurrentWindow()
        { 
            if (_getWindowInstanceFI == null)
            {
                _getWindowInstanceFI = typeof(TypeSearchDropdown).GetField("m_WindowInstance", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return (EditorWindow)_getWindowInstanceFI.GetValue(this);
        }
    }
}
