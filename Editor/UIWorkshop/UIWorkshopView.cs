using System;
using System.Linq;
using System.Reflection;
using DatastoresDX.Editor;
using DatastoresDX.Editor.DataCollections;
using DatastoresDX.Runtime;
using DatastoresDX.Runtime.DataCollections;
using MyMVVM.RuntimeInspect;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UICoreSystem.UIWorkshop
{
    public class UIWorkshopView : VisualElement
    {
        private static string VIEW_UXML = "UIWorkshop/UIWorkshopView";
        
        // Settings Area
        private static string STUDIO_SCENE_FIELD_TAG = "studio-scene-field";
        private static string UIPLATFORM_ENUM_TAG = "uiplatform-enum";
        private static string CURRENT_VIEW_DRAWER_TAG = "current-view-drawer";
        private static string VIEW_PREVIEW_RENDER_TAG = "view-preview-render";
        private static string VIEWMODEL_TYPE_LABEL_TAG = "viewmodel-type-label";
        private static string MOCK_VIEWMODEL_DROPDOWN_TAG = "mock-viewmodel-dropdown";
        private static string UIBUILDER_BUTTON_TAG = "uibuilder-button";
        private static string DEBUGGER_BUTTON_TAG = "debugger-button";
        
        // Edit Mode Area
        private static string EDITMODE_CONTAINER_TAG = "edit-mode-container";
        private static string PLAY_BUTTON_TAG = "play-button";
        
        // Play Mode Area
        private static string PLAYMODE_CONTAINER_TAG = "play-mode-container";
        private static string STOP_BUTTON_TAG = "stop-button";
        private static string RELOAD_BUTTON_TAG = "reload-button";
        private static string RELOAD_VIEW_ONLY_BUTTON_TAG = "reload-view-only-button";
        private static string VIEWMODELS_CONTAINER_TAG = "viewmodels-container";
        
        private ObjectField m_studioSceneField;
        private EnumField m_uiPlatformsEnum;
        private DataReferenceDrawer m_currentViewDrawer;
        private VisualElement m_viewPreviewRender;
        private Label m_viewModelTypeLabel;
        private DropdownField m_mockViewModelDropdown;
        private Button m_uiBuilderButton;
        private Button m_debuggerButton;
        
        private VisualElement m_editModeContainer;
        private Button m_playButton;
        
        private VisualElement m_playModeContainer;
        private Button m_stopButton;
        private Button m_reloadButton;
        private Button m_reloadViewOnlyButton;
        private VisualElement m_viewModelsContainer;
        private BaseModelInspector m_viewModelInspector;
        
        private UIWorkshopViewModel m_uiWorkshopViewModel;
        
        public UIWorkshopView(UIWorkshopViewModel uiWorkshopViewModel)
        {
            m_uiWorkshopViewModel = uiWorkshopViewModel;
            
            CreateLayout();
            CreateBindings();
            LoadSettings();
        }

        private void CreateLayout()
        {
            var uxmlAsset = Resources.Load<VisualTreeAsset>(VIEW_UXML);
            uxmlAsset.CloneTree(this);
            
            m_studioSceneField = this.Q<ObjectField>(STUDIO_SCENE_FIELD_TAG);
            m_uiPlatformsEnum = this.Q<EnumField>(UIPLATFORM_ENUM_TAG);
            m_currentViewDrawer = this.Q<DataReferenceDrawer>(CURRENT_VIEW_DRAWER_TAG);
            m_viewPreviewRender = this.Q<VisualElement>(VIEW_PREVIEW_RENDER_TAG);
            m_viewModelTypeLabel = this.Q<Label>(VIEWMODEL_TYPE_LABEL_TAG);
            m_mockViewModelDropdown = this.Q<DropdownField>(MOCK_VIEWMODEL_DROPDOWN_TAG);
            m_uiBuilderButton = this.Q<Button>(UIBUILDER_BUTTON_TAG);
            m_debuggerButton = this.Q<Button>(DEBUGGER_BUTTON_TAG);
            
            m_editModeContainer = this.Q<VisualElement>(EDITMODE_CONTAINER_TAG);
            m_playButton = this.Q<Button>(PLAY_BUTTON_TAG);
            
            m_playModeContainer = this.Q<VisualElement>(PLAYMODE_CONTAINER_TAG);
            m_stopButton = this.Q<Button>(STOP_BUTTON_TAG);
            m_reloadButton = this.Q<Button>(RELOAD_BUTTON_TAG);
            m_reloadViewOnlyButton = this.Q<Button>(RELOAD_VIEW_ONLY_BUTTON_TAG);
            m_viewModelsContainer = this.Q<VisualElement>(VIEWMODELS_CONTAINER_TAG);
            m_viewModelInspector = new BaseModelInspector();
            m_viewModelsContainer.Add(m_viewModelInspector);
        }

        private void CreateBindings()
        {
            m_uiPlatformsEnum.RegisterValueChangedCallback(OnUILayoutModeEnumDropdownChanged);
            m_uiWorkshopViewModel.UiPlatform_OnChanged += OnUIPlatformChanged;
            
            m_uiWorkshopViewModel.ShowPlayModeContainer_OnChanged += OnShowPlayModeContainerChanged;
            m_uiWorkshopViewModel.ViewToShow_OnChanged += OnCurrentUIWorkshopViewChanged;
            m_uiWorkshopViewModel.ViewModelType_OnChanged += OnViewModelTypeChanged;
            m_uiWorkshopViewModel.MockMethods_OnChanged += OnMockMethodsChanged;
            m_uiWorkshopViewModel.ViewInstance_OnChanged += OnViewInstanceChanged;
            
            m_studioSceneField.value = m_uiWorkshopViewModel.ViewStudioScene;
            m_currentViewDrawer.SetElementType(typeof(BaseViewData));
            m_currentViewDrawer.OnValueChanged += OnCurrentViewChanged;
            
            m_mockViewModelDropdown.RegisterValueChangedCallback(OnDropdownValueChanged);
            
            m_playButton.clicked += OnPlayButtonPressed;
            
            m_stopButton.clicked += OnStopButtonPressed;
            m_reloadButton.clicked += OnReloadButtonPressed;
            m_reloadViewOnlyButton.clicked += OnReloadViewOnlyButtonPressed;
            m_uiBuilderButton.clicked += OnUIBuilderButtonPressed;
            m_debuggerButton.clicked += OnDebuggerButtonPressed;
            
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void LoadSettings()
        {
            m_uiPlatformsEnum.SetValueWithoutNotify(m_uiWorkshopViewModel.UiPlatform);
            OnShowPlayModeContainerChanged(m_uiWorkshopViewModel.ShowPlayModeContainer);
            OnViewModelTypeChanged(m_uiWorkshopViewModel.ViewModelType);
            OnViewInstanceChanged(m_uiWorkshopViewModel.ViewInstance); // should be null
            
            string selectedMethod = m_uiWorkshopViewModel.SelectedMockMethod;
            OnMockMethodsChanged();
            if (m_mockViewModelDropdown.choices.Contains(selectedMethod)) // Restore saved dropdown value.
            {
                m_mockViewModelDropdown.value = selectedMethod;
            }

            OnCurrentUIWorkshopViewChanged(m_uiWorkshopViewModel.ViewToShow);
        }

        private void OnUILayoutModeEnumDropdownChanged(ChangeEvent<Enum> evt)
        {
            m_uiWorkshopViewModel.UiPlatform_Set((UIPlatform)evt.newValue);
        }
        
        private void OnUIPlatformChanged(UIPlatform value)
        {
            m_uiPlatformsEnum.SetValueWithoutNotify(m_uiWorkshopViewModel.UiPlatform);
            OnCurrentUIWorkshopViewChanged(m_uiWorkshopViewModel.ViewToShow);
        }

        private void OnShowPlayModeContainerChanged(bool showPlayMode)
        {
            m_editModeContainer.style.display = showPlayMode ? DisplayStyle.None : DisplayStyle.Flex;
            m_playModeContainer.style.display = showPlayMode ? DisplayStyle.Flex : DisplayStyle.None;

            // m_editModeContainer.SetEnabled(!showPlayMode);
            // m_playModeContainer.SetEnabled(showPlayMode);
            m_viewModelInspector.ClearDrawer();
        }
        
        private void OnCurrentViewChanged(Uid elementId)
        {
            m_uiWorkshopViewModel.ViewToShow_Set(new DataReference<BaseViewData>(elementId));
        }
        
        private void OnCurrentUIWorkshopViewChanged(DataReference<BaseViewData> obj)
        {
            m_viewPreviewRender.Clear();
            if (m_uiWorkshopViewModel.ViewToShow.DataElementId.IsInvalid())
            {
                return;
            }
            
            // Visually update the DataReference drawer.
            m_currentViewDrawer.SetDataElementId(m_uiWorkshopViewModel.ViewToShow.DataElementId, false);
            
            VisualTreeAsset visualTreeAsset = GetVisualTreeAsset();
            if (visualTreeAsset != null)
            {
                visualTreeAsset.CloneTree(m_viewPreviewRender);
            }
        }
        
        private void OnViewModelTypeChanged(string viewModelType)
        {
            m_viewModelTypeLabel.text = string.IsNullOrEmpty(viewModelType) ? "Unknown ViewModel" : viewModelType;
        }
        
        private void OnMockMethodsChanged()
        {
            m_mockViewModelDropdown.choices = m_uiWorkshopViewModel.MockMethods.ToList();
            if (m_mockViewModelDropdown.choices.Count > 0)
            {
                if (m_mockViewModelDropdown.choices.Contains(m_uiWorkshopViewModel.SelectedMockMethod))
                {
                    return;
                }
                
                m_mockViewModelDropdown.value = m_mockViewModelDropdown.choices[0];
                if (m_mockViewModelDropdown.value != m_uiWorkshopViewModel.SelectedMockMethod)
                {
                    m_uiWorkshopViewModel.SelectedMockMethod_Set(m_mockViewModelDropdown.value, false);
                }
            }
            else
            {
                m_mockViewModelDropdown.value = "";
            }
        }
        
        private void OnViewInstanceChanged(IView view)
        {
            m_viewModelInspector.ClearDrawer();
            
            if (view?.GetViewModel() != null)
            {
                m_viewModelInspector.SetModel(view.GetViewModel(), "ViewModel");
            }
        }

        private void OnDropdownValueChanged(ChangeEvent<string> evt)
        {
            m_uiWorkshopViewModel.SelectedMockMethod_Set(evt.newValue);
        }

        private void OnPlayButtonPressed()
        {
            EditorSceneManager.playModeStartScene = m_uiWorkshopViewModel.ViewStudioScene;
            EditorApplication.EnterPlaymode();
        }

        private void OnStopButtonPressed()
        {
            EditorApplication.ExitPlaymode();
        }
        
        private void OnReloadButtonPressed()
        {
            UIWorkshopBridge.ReloadView(true);
        }
        
        private void OnReloadViewOnlyButtonPressed()
        {
            UIWorkshopBridge.ReloadView(false);
        }
        
        private void OnUIBuilderButtonPressed()
        {
            VisualTreeAsset visualTreeAsset = GetVisualTreeAsset();
            if (visualTreeAsset != null)
            {
                AssetDatabase.OpenAsset(visualTreeAsset);
            }
        }

        private VisualTreeAsset GetVisualTreeAsset()
        {
            if (m_uiWorkshopViewModel.ViewToShow.DataElementId.IsInvalid())
            {
                return null;
            }
            
            WorkflowElementKey elementKey = DatastoresEditorCore.GetDataElementKey(m_uiWorkshopViewModel.ViewToShow.DataElementId, true);
            if (elementKey.IsInvalid())
            {
                return null;
            }
            SerializedProperty elementSP = ((DataCollectionElementWrapper)elementKey.GetElement()).ElementSP;

            string propertyName = "";
            if (m_uiWorkshopViewModel.UiPlatform == UIPlatform.PC || m_uiWorkshopViewModel.UiPlatform == UIPlatform.Console)
            {
                propertyName = BaseViewData.PCConsoleUXML_VarName;
            }
            else if (m_uiWorkshopViewModel.UiPlatform == UIPlatform.Mobile)
            {
                propertyName = BaseViewData.MobileUXML_VarName;
            }
            SerializedProperty uxmlSP = elementSP.FindPropertyRelative(propertyName);
            string uxmlGuid = uxmlSP.FindPropertyRelative(BaseSoftRef.Guid_VarName).stringValue;
            if (string.IsNullOrEmpty(uxmlGuid))
            {
                Debug.LogError($"UXML for Platform {m_uiWorkshopViewModel.UiPlatform} did not have a UXML.");
                return null;
            }
                    
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(uxmlGuid));
        }
        
        private void OnDebuggerButtonPressed()
        {
            Assembly ass = Assembly.Load("UnityEditor.UIElementsModule");
            Type debuggerWindowType = ass.GetType("UnityEditor.UIElements.Debugger.UIElementsDebugger");
            EditorWindow.GetWindow(debuggerWindowType);
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                EditorSceneManager.playModeStartScene = null;
            }
        }
    }
}