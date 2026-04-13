using System;
using DatastoresDX.Runtime;
using UICoreSystem.Utility;
using UnityEngine;

namespace UICoreSystem
{
    [DataElement(typeof(ViewCollection), "ScreenData")]
    public class ScreenData : AViewData<IScreen>
    {
        [Header("Base")]
        [SerializeField, TypeReference(typeof(IScreen), true)]
        private TypeReference m_viewType;
        public override Type ViewType => m_viewType.Get();
    }
}