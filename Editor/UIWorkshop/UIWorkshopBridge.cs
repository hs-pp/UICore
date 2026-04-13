using System;
using DatastoresDX.Runtime;
using MyMVVM;
using SystemCoreSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace UICoreSystem.UIWorkshop
{
    public static class UIWorkshopBridge
    {
        private static string MODEL_SAVE_KEY = "UIWorkshopModel";
        
        public static UIWorkshopModel UIWorkshopModel { get; private set; }
        public static UIWorkshopController RuntimeController { get; private set; }

        static UIWorkshopBridge()
        {
            LoadModel();
            
            AssemblyReloadEvents.beforeAssemblyReload -= SaveModel;
            AssemblyReloadEvents.beforeAssemblyReload += SaveModel;
            
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                OnEnterPlayMode();
            }
            else
            {
                OnExitPlayMode();
            }
            UIWorkshopModel.IsPlayMode_Set(state == PlayModeStateChange.EnteredPlayMode);
        }
        
        private static void OnEnterPlayMode()
        {
            // We're not in the UIWorkshop scene. Do nothing.
            if (SceneManager.GetActiveScene().name != "UIWorkshop")
            {
                return;
            }
            
            // Try to get the RuntimeController.
            RuntimeController = Object.FindAnyObjectByType<UIWorkshopController>();
            if (RuntimeController == null)
            {
                Debug.LogWarning("[UIWorkshop] Could not find view studio controller! Not setting up bridge!");
                return;
            }
            RuntimeController.OnViewInstanceChanged += OnViewInstanceChanged;
            
            // Waiting for SystemCore also covers Datastores loading.
            SystemCore.OnKeySystemsInitializedOrImmediate(() =>
            {
                OnSelectedMockMethodChanged(UIWorkshopModel.SelectedMockMethod);
            });
        }

        private static void OnExitPlayMode()
        {
            RuntimeController = null;
        }
        
        private static void LoadModel()
        {
            string json = EditorPrefs.GetString(MODEL_SAVE_KEY);
            if (!string.IsNullOrEmpty(json))
            {
                UIWorkshopModel = JsonUtility.FromJson<UIWorkshopModel>(json);
            }
            else
            {
                UIWorkshopModel = new UIWorkshopModel();
            }
            
            UIWorkshopModel.LoadMockMethods();
            UIWorkshopModel.SelectedMockMethod_OnChanged += OnSelectedMockMethodChanged;
            UIWorkshopModel.UiPlatform_OnChanged += OnUiPlatformChanged;
        }

        private static void SaveModel()
        {
            EditorPrefs.SetString(MODEL_SAVE_KEY, JsonUtility.ToJson(UIWorkshopModel));
        }

        private static void OnSelectedMockMethodChanged(string selectedMockMethod)
        {
            if (RuntimeController == null)
            {
                return;
            }
            
            BaseViewData viewData = Datastores.GetElement(UIWorkshopModel.ViewToShow.DataElementId) as BaseViewData;
            if (viewData == null)
            {
                return;
            }
            
            RuntimeController.SetView(viewData, GetMockViewModel(viewData));
        }
        
        private static void OnUiPlatformChanged(UIPlatform platform)
        {
            if (RuntimeController != null)
            {
                RuntimeController.SetUILayoutMode(platform);
            }
        }
        
        private static void OnViewInstanceChanged(IView view)
        {
            UIWorkshopModel.ViewInstance_Set(view);
        }
        
        public static void ReloadView(bool reloadViewModel)
        {
            if (RuntimeController == null)
            {
                return;
            }

            BaseViewData viewData = Datastores.GetElement(UIWorkshopModel.ViewToShow.DataElementId) as BaseViewData;

            RuntimeController.SetView(viewData, reloadViewModel ? GetMockViewModel(viewData) : UIWorkshopModel.ViewInstance.GetViewModel());
        }

        private static ViewModel GetMockViewModel(BaseViewData viewData)
        {
            if (viewData == null)
            {
                return null;
            }
            
            // Get the parent AView type.
            Type aViewType = viewData.ViewType;
            while (aViewType != null && !(aViewType.IsGenericType && aViewType.GetGenericTypeDefinition() == typeof(AView<>)))
            {
                aViewType = aViewType.BaseType;
            }
            Type viewModelType = aViewType.GetGenericArguments()[0];
            return ViewModelMockProvider.GetMockWithMethod(viewModelType, UIWorkshopModel.SelectedMockMethod);
        }
    }
}