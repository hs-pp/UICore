using UICoreSystem;
using UnityEngine.UIElements;

/// <summary>
/// Only to be used in UIWorkshop to show standalone AViews.
/// </summary>
public class ViewDisplayerScreen : AScreenWithNoViewModel
{
    protected override void OnCreateLayout() { }

    public override ScreenType ScreenType => ScreenType.Unsorted;

    private IView m_displayView;
    
    public override void Initialize()
    {
        // Don't do the default stuff.
    }
    
    public ViewDisplayerScreen(IView view)
    {
        Add((VisualElement)view);
        m_displayView = view;
    }
}
