using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatastoresDX.Runtime;
using MyMVVM;
using SystemCoreSystem;
using UICoreSystem;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using DatastoresDX.Editor;
using DatastoresDX.Editor.DataCollections;
#endif

public class UICore : AKeySystem
{
    private const string PRIMARYLAYOUTSPAWNER_PREFAB_PATH = "UICore/PrimaryLayoutSpawner";
    private PrimaryLayout m_primaryLayout;
    public static UIPlatform CurrentPlatform { get; private set; } = UIPlatform.PC;
    
    public override Task OnInitialize()
    {
        CacheViewDatas();
        SpawnPrimaryLayout();
        
        return Task.CompletedTask;
    }

    public override Task OnDeinitialize()
    {
        ClearAndDestroyScreens();
        return base.OnDeinitialize();
    }

    public void SetUIPlatform(UIPlatform platform)
    {
        if (CurrentPlatform != platform)
        {
            CurrentPlatform = platform;
            Debug.Log("UICore set UIPlatform to " + platform);
            // do something bro
        }
    }
  
#region PrimaryLayout
    private void SpawnPrimaryLayout()
    {
        PrimaryLayoutSpawner spawnerPrefab = Resources.Load<PrimaryLayoutSpawner>(PRIMARYLAYOUTSPAWNER_PREFAB_PATH);
        if (spawnerPrefab == null)
        {
            Debug.LogError("PrimaryLayout Spawner prefab is null! Could not instantiate!");
            return;
        }

        PrimaryLayoutSpawner primaryLayoutSpawner = Instantiate(spawnerPrefab, transform);
        m_primaryLayout = primaryLayoutSpawner.PrimaryLayout;
    }
    
    public static T CreateScreen<T>(ViewModel viewModel) where T : IScreen
    {
        T view = (T)CreateScreen(typeof(T), viewModel);
        return view;
    }
    
    public static IScreen CreateScreen(Type screenType, ViewModel viewModel)
    {
        if(screenType.IsSubclassOf(typeof(AScreenWithNoViewModel)))
        {
            viewModel = new EmptyViewModel();
        }
        
        return CreateView(screenType, viewModel) as IScreen;
    }
    
    public void AddScreen(IScreen screen)
    {
        m_primaryLayout.AddScreen(screen);
    }

    public void RemoveScreen(IScreen screen, bool cleanup = false)
    {
        m_primaryLayout.RemoveScreen(screen);
        if (cleanup)
        {
            screen.Cleanup();
        }
    }

    public T GetScreen<T>() where T : IScreen
    {
        return m_primaryLayout.GetScreen<T>();
    }

    public IScreen GetScreen(Type type)
    {
        return m_primaryLayout.GetScreen(type);
    }

    public bool ScreenExists<T>() where T : IScreen
    {
        return GetScreen<T>() != null;
    }

    public bool ScreenExists(Type type)
    {
        return GetScreen(type) != null;
    }

    public void BringScreenToFront(IScreen screen)
    {
        m_primaryLayout.BringScreenToFront(screen);
    }

    public void ClearAndDestroyScreens()
    {
        foreach (IScreen screen in m_primaryLayout.GetAllScreens())
        {
            RemoveScreen(screen, true);
        }
    }
#endregion

#region Core
    // Cached View types
    private static Dictionary<Type, BaseViewData> m_cachedViewDatas = new();

    private void CacheViewDatas()
    {
        m_cachedViewDatas.Clear();
        foreach (BaseViewData viewData in Datastores.GetElementsOfType<BaseViewData>())
        {
            m_cachedViewDatas.Add(viewData.ViewType, viewData);

            Type pcConsoleType = viewData.PCConsoleViewType.Get();
            if (pcConsoleType != null && !m_cachedViewDatas.ContainsKey(pcConsoleType))
            {
                m_cachedViewDatas.Add(pcConsoleType, viewData);
            }
            
            Type mobileType = viewData.MobileViewType.Get();
            if (mobileType != null && !m_cachedViewDatas.ContainsKey(mobileType))
            {
                m_cachedViewDatas.Add(mobileType, viewData);
            }
        }
    }

    public static T CreateView<T>(ViewModel viewModel) where T : IView
    {
        return (T)CreateView(typeof(T), viewModel);
    }

    public static IView CreateView(Type viewType, ViewModel viewModel)
    {
        if (!typeof(IView).IsAssignableFrom(viewType))
        {
            return null;
        }
        
        if(viewType.IsSubclassOf(typeof(AViewWithNoViewModel)))
        {
            viewModel = new EmptyViewModel();
        }

        IView view = Activator.CreateInstance(viewType) as IView;
        view.SetViewModel(viewModel);
        
        return view;
    }
    
    public static bool IsInFocusPath(VisualElement element)
    {
        if (element.focusController == null)
        {
            return false;
        }
            
        VisualElement focusPath = element.focusController.focusedElement as VisualElement;
        while (focusPath != null)
        {
            if (focusPath == element)
            {
                return true;
            }

            focusPath = focusPath.parent;
        }

        return false;
    }

    public static BaseViewData GetViewData(Type viewType)
    {
        BaseViewData toReturn = null;
#if UNITY_EDITOR
        if (!Application.isPlaying) // Check this way if we're in editor and not in play mode.
        {
            List<WorkflowElementKey> elementKeys = DatastoresEditorCore.GetAllDataElementsOfType(typeof(BaseViewData));
            foreach (WorkflowElementKey elementKey in elementKeys)
            {
                toReturn = (elementKey.GetElement() as DataCollectionElementWrapper).RuntimeElement as BaseViewData;
            }
        }
#endif
        if (Application.isPlaying)
        {
            m_cachedViewDatas.TryGetValue(viewType, out BaseViewData data);
            toReturn = data;
        }

        if (toReturn == null)
        {
            Debug.LogError($"Could not get a ViewData for {viewType}!");
        }
        return toReturn;
    }
#endregion
}