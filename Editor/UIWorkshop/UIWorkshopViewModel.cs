using System.Collections.Generic;
using DatastoresDX.Runtime;
using MyMVVM;
using UnityEditor;
using UnityEngine;

namespace UICoreSystem.UIWorkshop
{
    public partial class UIWorkshopViewModel : ViewModel
    {
        public SceneAsset ViewStudioScene { get; }
        
        [Passthrough("m_model", "m_uiPlatform")]
        private UIPlatform m_uiPlatform;
        
        [Passthrough("m_model", "m_isPlayMode")]
        private bool m_showPlayModeContainer;
        
        [Passthrough("m_model", "m_viewToShow")]
        private DataReference<BaseViewData> m_viewToShow;
        
        [Passthrough("m_model", "m_viewModelType")]
        private string m_viewModelType;
        
        [Passthrough("m_model", "m_mockMethods")]
        private List<string> m_mockMethods;
        
        [Passthrough("m_model", "m_selectedMockMethod")]
        private string m_selectedMockMethod;
        
        [Passthrough("m_model", "m_viewInstance")]
        private IView m_viewInstance;
        
        private UIWorkshopModel m_model;
        
        public UIWorkshopViewModel()
        {
            m_model = UIWorkshopBridge.UIWorkshopModel;
            ViewStudioScene = Resources.Load<SceneAsset>(UIWorkshopModel.StudioScenePath);
        }
    }
}