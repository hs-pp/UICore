using System;
using MyMVVM;
using SystemCoreSystem;
using UnityEngine;

namespace UICoreSystem.UIWorkshop
{
    public class UIWorkshopController : MonoBehaviour
    {
        private UICore m_uiCore;
        private IView m_activeViewInstance;
        public Action<IView> OnViewInstanceChanged;

        public void Awake()
        {
            SystemCore.OnKeySystemsInitializedOrImmediate(() =>
            {
                m_uiCore = SystemCore.GetKeySystem<UICore>();
            });
        }

        public void SetUILayoutMode(UIPlatform platform)
        {
            m_uiCore.SetUIPlatform(platform);
        }

        public void SetView(BaseViewData viewData, ViewModel viewModel)
        {
            RemoveExistingView();
            if (typeof(IScreen).IsAssignableFrom(viewData.ViewType))
            {
                m_activeViewInstance = UICore.CreateScreen(viewData.ViewType, viewModel);
                m_uiCore.AddScreen((IScreen)m_activeViewInstance);
            }
            else if (typeof(IView).IsAssignableFrom(viewData.ViewType))
            {
                m_activeViewInstance = UICore.CreateView(viewData.ViewType, viewModel);
                ViewDisplayerScreen viewDisplayerScreen = new ViewDisplayerScreen(m_activeViewInstance);
                m_uiCore.AddScreen(viewDisplayerScreen);
            }
            OnViewInstanceChanged?.Invoke(m_activeViewInstance);
        }

        private void RemoveExistingView()
        {
            if (m_activeViewInstance != null)
            {
                if (m_activeViewInstance is IScreen screen && m_uiCore.ScreenExists(m_activeViewInstance.GetType()))
                {
                    m_uiCore.RemoveScreen(screen);
                }
                else
                {
                    ViewDisplayerScreen viewDisplayerScreen = m_uiCore.GetScreen<ViewDisplayerScreen>();
                    m_uiCore.RemoveScreen(viewDisplayerScreen);
                }
            }

            m_activeViewInstance = null;
            OnViewInstanceChanged?.Invoke(null);
        }
    }
}