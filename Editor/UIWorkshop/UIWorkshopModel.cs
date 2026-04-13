using System;
using System.Collections.Generic;
using DatastoresDX.Editor;
using DatastoresDX.Editor.DataCollections;
using DatastoresDX.Runtime;
using MyMVVM;
using UnityEngine;

namespace UICoreSystem.UIWorkshop
{
    [Serializable]
    public partial class UIWorkshopModel : Model
    {
        public const string StudioScenePath = "UIWorkshop/UIWorkshop";
        
        [SerializeField, Observable]
        private UIPlatform m_uiPlatform = UIPlatform.PC;
        
        [SerializeField, Observable]
        private bool m_isPlayMode;

        [SerializeField, Observable]
        private DataReference<BaseViewData> m_viewToShow;
        
        [SerializeField, Observable]
        private string m_viewModelType;
        
        [Observable]
        private List<string> m_mockMethods = new();

        [SerializeField, Observable]
        private string m_selectedMockMethod;

        // While in playmode.
        [Observable]
        private IView m_viewInstance;

        public UIWorkshopModel()
        {
            m_ViewToShow_OnChanged += OnViewToShowChanged;
        }

        private void OnViewToShowChanged(DataReference<BaseViewData> obj)
        {
            LoadMockMethods();
        }

        public void LoadMockMethods()
        {
            IDataElement dataElement = DatastoresEditorCore.GetDataElementKey(m_viewToShow.DataElementId).GetElement();
            if (dataElement == null)
            {
                MockMethods_Set(new());
                return;
            }
            
            Type viewType = ((dataElement as DataCollectionElementWrapper).RuntimeElement as BaseViewData).ViewType;
            
            // Get the parent AView type.
            Type aViewType = viewType;
            while (aViewType != null && !(aViewType.IsGenericType && aViewType.GetGenericTypeDefinition() == typeof(AView<>)))
            {
                aViewType = aViewType.BaseType;
            }

            if (aViewType == null)
            {
                MockMethods_Set(ViewModelMockProvider.GetMockMethodNames(null));
                return;
            }

            Type viewModelType = aViewType.GetGenericArguments()[0];
            ViewModelType_Set(viewModelType.Name);
            MockMethods_Set(ViewModelMockProvider.GetMockMethodNames(viewModelType));
        }
    }
}