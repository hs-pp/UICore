using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UICoreSystem.Utility.Editor
{
    public class TypeSearchPopupContent : PopupWindowContent
    {
        private const string VIEW_UXML = "UICore/Utility/TypeSearchPopupContent";
        private const string TYPES_LIST_TREEVIEW_TAG = "types-list-treeview";
        private const string CLEAR_BUTTON_TAG = "clear-button";

        private TreeView m_typesListTreeview;
        private Button m_clearButton;

        private SerializedProperty m_property;
        private TypeReferenceAttribute m_attr;
        private Rect m_worldBounds;
        private Action<Type> m_callback;
        private int m_idCounter = 0;

        public TypeSearchPopupContent(SerializedProperty property, TypeReferenceAttribute attr, Rect worldBounds,
            Action<Type> callback)
        {
            m_property = property;
            m_attr = attr;
            m_worldBounds = worldBounds;
            m_callback = callback;
        }

        public override VisualElement CreateGUI()
        {
            VisualElement popupElement = new VisualElement();
            popupElement.style.width = m_worldBounds.width;
            popupElement.style.height = 300;

            var uxmlAsset = Resources.Load<VisualTreeAsset>(VIEW_UXML);
            uxmlAsset.CloneTree(popupElement);

            m_typesListTreeview = popupElement.Q<TreeView>(TYPES_LIST_TREEVIEW_TAG);
            m_clearButton = popupElement.Q<Button>(CLEAR_BUTTON_TAG);

            m_clearButton.clicked += () =>
            {
                m_callback?.Invoke(null);
                editorWindow.Close();
            };

            m_typesListTreeview.makeItem = () => { return new TypeListElement(); };
            m_typesListTreeview.bindItem = (visualElement, index) =>
            {
                (visualElement as TypeListElement).SetType(
                    m_typesListTreeview.GetItemDataForIndex<TypeData>(index));
            };
            m_typesListTreeview.itemsChosen += objects =>
            {
                if (objects.Count() == 1)
                {
                    TypeData typeData = objects.First() as TypeData;
                    if (typeData.IsSelectable)
                    {
                        m_callback?.Invoke(typeData.Type);
                        editorWindow.Close();
                    }
                }
            };

            // Set data
            m_typesListTreeview.SetRootItems(new List<TreeViewItemData<TypeData>>() { GetTypeTree(m_attr.BaseType) });
            m_typesListTreeview.ExpandAll();

            return popupElement;
        }

        private TreeViewItemData<TypeData> GetTypeTree(Type baseType)
        {
            m_idCounter = 0;

            // Get all loaded types that are subclass of baseType (or implement the interface)
            var allDerivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t != null && baseType.IsAssignableFrom(t) && t != baseType)
                .ToList();

            // Group by their direct base type
            var typeMap = new Dictionary<Type, List<Type>>();
            foreach (var type in allDerivedTypes)
            {
                Type parentType = allDerivedTypes.Contains(type.BaseType) ? type.BaseType : null;
                if (parentType == null && type.BaseType != null &&
                    type.BaseType.IsGenericType) // maybe it's a generic type
                {
                    Type genericType = type.BaseType.GetGenericTypeDefinition();
                    parentType = allDerivedTypes.Contains(genericType) ? genericType : null;
                }

                if (parentType == null) // it could be an interface
                {
                    parentType = type.GetInterfaces().FirstOrDefault(i => allDerivedTypes.Contains(i));
                }

                if (parentType == null)
                {
                    parentType = baseType;
                }

                if (!typeMap.ContainsKey(parentType))
                {
                    typeMap[parentType] = new List<Type>();
                }

                typeMap[parentType].Add(type);
            }

            // Recursive tree build
            return BuildNodeRecursive(baseType, typeMap);
        }

        private TreeViewItemData<TypeData> BuildNodeRecursive(Type current, Dictionary<Type, List<Type>> typeMap)
        {
            var id = m_idCounter++;
            var children = new List<TreeViewItemData<TypeData>>();

            if (typeMap.TryGetValue(current, out var subTypes))
            {
                foreach (var subtype in subTypes.OrderBy(t => t.Name))
                {
                    children.Add(BuildNodeRecursive(subtype, typeMap));
                }
            }

            bool isSelectable = true;
            if (!m_attr.AllowAbstractAndInterfaces && (current.IsAbstract || current.IsInterface))
            {
                isSelectable = false;
            }

            return new TreeViewItemData<TypeData>(id, new TypeData() { Type = current, IsSelectable = isSelectable },
                children);
        }
    }

    public class TypeData
    {
        public Type Type;
        public bool IsSelectable;
    }

    public class TypeListElement : VisualElement
    {
        private Label m_label;
        private VisualElement m_disabledElement;

        public TypeListElement()
        {
            style.height = 22;
            m_label = new Label();
            m_label.style.flexGrow = 1;
            m_label.style.unityTextAlign = TextAnchor.MiddleLeft;
            Add(m_label);

            m_disabledElement = new VisualElement();
            m_disabledElement.style.position = Position.Absolute;
            m_disabledElement.style.left = 0;
            m_disabledElement.style.right = 0;
            m_disabledElement.style.top = 0;
            m_disabledElement.style.bottom = 0;
            m_disabledElement.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            Add(m_disabledElement);
        }

        public void SetType(TypeData typeData)
        {
            if (typeData.Type.IsGenericType)
            {
                m_label.text = $"{GetGenericTypeName(typeData.Type.GetGenericTypeDefinition())}<>";
            }
            else
            {
                m_label.text = typeData.Type.Name;
            }

            m_disabledElement.style.display = typeData.IsSelectable ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private string GetGenericTypeName(Type type)
        {
            string name = type.Name;
            int index = name.IndexOf('`');
            return index > 0 ? name.Substring(0, index) : name;
        }
    }
}