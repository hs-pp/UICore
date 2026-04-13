using MyMVVM;
using UnityEngine.UIElements;

namespace UICoreSystem
{
    public interface IScreen : IView
    {
        ScreenType ScreenType { get; }
        void OnAddedToPrimaryLayout() {}
        void OnRemovedFromPrimaryLayout() {}
    }
    
    public abstract class AScreen<T> : AView<T>, IScreen  where T : ViewModel
    {
        public abstract ScreenType ScreenType { get; }
        
        public AScreen()
        {
            pickingMode = PickingMode.Ignore;
        }

        public virtual void OnAddedToPrimaryLayout() {}
        public virtual void OnRemovedFromPrimaryLayout() {}
    }
    
    public abstract class AScreenWithNoViewModel : AScreen<EmptyViewModel> { }
}