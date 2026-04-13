using System;
using DatastoresDX.Runtime;
using UICoreSystem.Utility;
using UnityEngine;

namespace UICoreSystem
{
    [DataElement(typeof(ViewCollection), "ViewData")]
    public class ViewData : AViewData<IView>
    {
        [Header("Base")]
        [SerializeField, TypeReference(typeof(IView), true)]
        private TypeReference m_viewType;
        public override Type ViewType => m_viewType.Get();
    }
}