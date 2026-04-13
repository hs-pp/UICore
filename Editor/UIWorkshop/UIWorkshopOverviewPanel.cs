using DatastoresDX.Editor;
using UnityEngine.UIElements;

namespace UICoreSystem.UIWorkshop
{
    [OverviewPanel(typeof(ViewCollection))]
    public class UIWorkshopOverviewPanel : AOverviewPanel
    {
        protected override VisualElement CreatePanel()
        {
            return new UIWorkshopView(new UIWorkshopViewModel());
        }

        protected override void OnSetWorkflow(AWorkflow workflow) { }
    }
}