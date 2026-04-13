using System;
using UnityEngine;

namespace UICoreSystem.Utility
{
    [Serializable]
    public struct TypeReference
    {
        [SerializeField]
        private string m_typeNameAndAssembly;

        [NonSerialized]
        private Type m_loadedType;

        public TypeReference(Type type)
        {
            m_loadedType = type;
            m_typeNameAndAssembly = $"{type.FullName}, {type.Assembly.GetName()}";
        }

        public Type Get()
        {
            if (m_loadedType == null)
            {
                m_loadedType = Type.GetType(m_typeNameAndAssembly);
            }

            return m_loadedType;
        }

#if UNITY_EDITOR
        public static string TypeNameAndAssembly_VarName = "m_typeNameAndAssembly";
#endif
    }

    public class TypeReferenceAttribute : Attribute
    {
        public Type BaseType { get; }
        public bool AllowAbstractAndInterfaces { get; }

        public TypeReferenceAttribute(Type baseType, bool allowAbstractAndInterfaces)
        {
            BaseType = baseType;
            AllowAbstractAndInterfaces = allowAbstractAndInterfaces;
        }
    }
}