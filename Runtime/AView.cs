using System.Collections.Generic;
using System.Linq;
using MyMVVM;
using RemedySystem;
using UnityEngine.UIElements;

namespace UICoreSystem
{
    public interface IView
    {
        void Initialize();
        
        void SetViewModel(ViewModel viewModel);
        void UnsetViewModel();
        ViewModel GetViewModel();
        
        void SetStyles(List<StyleSheet> desiredStyleSheets);
        
        void Cleanup();
    }
    
    /// <summary>
    /// The most basic of visual elements that utilize the PrimaryLayout system.
    /// </summary>
    public abstract class ABaseView : VisualElement, IView
    {
        private List<StyleSheet> m_activeStyleSheets = new();
        protected BaseViewData m_viewData;
        
        public ABaseView()
        {
            Initialize();
        }
        
        public virtual void Initialize()
        {
            m_viewData = UICore.GetViewData(GetType());
            if (m_viewData == null)
            {
                return;
            }
            
            if (UICore.CurrentPlatform == UIPlatform.PC || UICore.CurrentPlatform == UIPlatform.Console)
            {
                CreateLayout(m_viewData.PCConsoleUXML.Get());
            }
            else if (UICore.CurrentPlatform == UIPlatform.Mobile)
            {
                CreateLayout(m_viewData.MobileUXML.Get());
            }
        }
        
        private void CreateLayout(VisualTreeAsset uxml)
        {
            if (uxml != null)
            {
                uxml.CloneTree(this);
                OnCreateLayout();
            }
        }
        
        public void SetStyles(List<StyleSheet> desiredStyleSheets)
        {
            IEnumerable<StyleSheet> changes = m_activeStyleSheets.Except(desiredStyleSheets).Concat(desiredStyleSheets.Except(m_activeStyleSheets));
            foreach (StyleSheet styleSheet in changes)
            {
                if (styleSheets.Contains(styleSheet))
                {
                    styleSheets.Remove(styleSheet);
                }
                else
                {
                    styleSheets.Add(styleSheet);
                }
            }
        }
        
        public void Cleanup()
        {
            OnCleanup();
            UnsetViewModel();
        }
        
        protected abstract void OnCreateLayout();
        public abstract void SetViewModel(ViewModel viewModel);
        public abstract void UnsetViewModel();
        public abstract ViewModel GetViewModel();
        protected virtual void OnCleanup() {}
    }
    
    public abstract class AView<T> : ABaseView where T : ViewModel
    {
        protected T m_viewModel;
        
        public sealed override void SetViewModel(ViewModel viewModel)
        {
            if (m_viewModel == viewModel)
            {
                return;
            }
            
            if (m_viewModel != null)
            {
                UnsetViewModel();
            }

            if (viewModel != null)
            {
                m_viewModel = viewModel as T;
                if (m_viewModel == null)
                {
                    Remedy.LogError(RemedyTypes.UICore, $"{GetType().Name} ViewModel type mismatch! Was expecting {typeof(T)} but got {viewModel.GetType().Name}!");
                    return;
                }
            }
            else
            {
                Remedy.LogError(RemedyTypes.UICore, $"Trying to set ViewModel for {GetType().Name} but it's null!");
            }
            
            OnSetViewModel(m_viewModel);
        }

        public sealed override void UnsetViewModel()
        {
            if (m_viewModel == null)
            {
                return;
            }
            
            OnUnsetViewModel();
            m_viewModel = null;
        }

        public sealed override ViewModel GetViewModel()
        {
            return m_viewModel;
        }
        
        protected virtual void OnSetViewModel(T viewModel) {}
        protected virtual void OnUnsetViewModel() {}
    }

    public abstract class AViewWithNoViewModel : AView<EmptyViewModel> { }
}