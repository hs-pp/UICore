using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace UICoreSystem.Utility.Editor
{
    [CustomPropertyDrawer(typeof(TypeReference), true)]
    public class TypeReferencePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new TypeReferenceField(property, new TypeReferenceElement(property, fieldInfo));
        }
    }

    public class TypeReferenceField : BaseField<TypeReference>
    {
        public TypeReferenceField(SerializedProperty property, TypeReferenceElement visualInput) : base(
            property.displayName, visualInput)
        {
            AddToClassList(alignedFieldUssClassName);
        }
    }

    public class TypeReferenceElement : VisualElement
    {
        private const string VIEW_UXML = "UICore/Utility/TypeReferenceElement";
        private const string CONTAINER_TAG = "container";
        private const string TYPE_LABEL_TAG = "type-label";

        private const string NONE_LABEL = "-none-";
        private const string ERROR_LABEL = "-error-";

        private VisualElement m_container;
        private Label m_typeLabel;

        private SerializedProperty m_property;
        private TypeReferenceAttribute m_propertyAttr;

        public TypeReferenceElement(SerializedProperty property, FieldInfo fieldInfo)
        {
            m_property = property;
            m_propertyAttr = fieldInfo.GetCustomAttribute<TypeReferenceAttribute>();

            CreateLayout();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(VIEW_UXML);
            uxmlAsset.CloneTree(this);

            m_container = this.Q<VisualElement>(CONTAINER_TAG);
            m_typeLabel = this.Q<Label>(TYPE_LABEL_TAG);

            this.AddManipulator(new Clickable(() =>
            {
                if (m_propertyAttr == null)
                {
                    Debug.LogError("Could not find TypeReference attribute.");
                    return;
                }

                if (m_propertyAttr.BaseType == null)
                {
                    Debug.LogError("BaseType specified in TypeReference attribute is null!");
                    return;
                }

                PopupWindow.Show(worldBound,
                    new TypeSearchPopupContent(m_property, m_propertyAttr, worldBound, SetProperty));
            }));

            LoadProperty();
        }

        private void LoadProperty()
        {
            SerializedProperty fieldProp = m_property.FindPropertyRelative(TypeReference.TypeNameAndAssembly_VarName);
            Type currentType = Type.GetType(fieldProp.stringValue);
            if (currentType != null)
            {
                m_typeLabel.text = currentType.Name;
            }
            else if (!string.IsNullOrEmpty(fieldProp.stringValue))
            {
                m_typeLabel.text = ERROR_LABEL;
            }
            else
            {
                m_typeLabel.text = NONE_LABEL;
            }

            SetStyle();
        }

        private void SetProperty(Type type)
        {
            SerializedProperty fieldProp = m_property.FindPropertyRelative(TypeReference.TypeNameAndAssembly_VarName);
            if (type == null)
            {
                fieldProp.stringValue = "";
                m_typeLabel.text = NONE_LABEL;
            }
            else
            {
                fieldProp.stringValue = $"{type.FullName}, {type.Assembly.GetName()}";
                m_typeLabel.text = type.Name;
            }

            m_property.serializedObject.ApplyModifiedProperties();

            SetStyle();
        }

        private void SetStyle()
        {
            if (m_typeLabel.text == ERROR_LABEL)
            {
                m_container.style.backgroundColor = Color.red;
            }
            else
            {
                m_container.style.backgroundColor = Color.clear;
            }
        }
    }
}