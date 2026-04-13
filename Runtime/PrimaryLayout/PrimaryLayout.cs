using System;
using System.Collections.Generic;
using RemedySystem;
using UnityEngine.UIElements;

namespace UICoreSystem
{
    /// <summary>
    /// Ordered by Priority for focus.
    /// </summary>
    public enum ScreenType : int
    {
        Unsorted = 0,
        GameUI = 1,
        Menu = 2,
        Transition = 3,
        Debug = 4,
    }

    [UxmlElement]
    [RemedyType("PrimaryLayout")]
    public partial class PrimaryLayout : VisualElement
    {
        private Dictionary<ScreenType, VisualElement> m_screenContainers = new();
        private IScreen m_screenWithFocus;
        
        public PrimaryLayout()
        {
            foreach (ScreenType screenType in Enum.GetValues(typeof(ScreenType)))
            {
                VisualElement container = new VisualElement();
                container.name = screenType.ToString();
                container.pickingMode = PickingMode.Ignore;

                m_screenContainers.Add(screenType, container);
                Add(container);

                container.style.position = Position.Absolute;
                container.style.top = 0;
                container.style.bottom = 0;
                container.style.left = 0;
                container.style.right = 0;
            }
        }

        public void AddScreen(IScreen screen)
        {
            if (screen == null)
            {
                Remedy.LogError(RemedyTypes.PrimaryLayout, "Screen being added is null!");
                return;
            }

            VisualElement screenElement = (VisualElement)screen;
            VisualElement screenContainer = m_screenContainers[screen.ScreenType];
            if (screenContainer.Contains(screenElement))
            {
                Remedy.LogWarning(RemedyTypes.PrimaryLayout, LogVerbosity.Normal, $"Screen {screen.GetType().Name} already added to PrimaryLayout!");
                return;
            }

            screenContainer.Add(screenElement);

            // Set screen to take up full screen.
            screenElement.style.position = Position.Absolute;
            screenElement.style.top = 0;
            screenElement.style.bottom = 0;
            screenElement.style.left = 0;
            screenElement.style.right = 0;

            screen.OnAddedToPrimaryLayout();
        }

        public void RemoveScreen(IScreen screen)
        {
            if (screen == null)
            {
                return;
            }

            VisualElement screenElement = (VisualElement)screen;
            VisualElement screenContainer = m_screenContainers[screen.ScreenType];
            if (!screenContainer.Contains(screenElement))
            {
                Remedy.LogWarning(RemedyTypes.PrimaryLayout, LogVerbosity.Normal,
                    $"Screen {screen.GetType().Name} was never added to PrimaryLayout but is trying to be removed!");
                return;
            }

            screenContainer.Remove(screenElement);
            screen.OnRemovedFromPrimaryLayout();
        }

        // GetScreen always loops through everything. But we shouldn't have too many screens anyway...
        public T GetScreen<T>() where T : IScreen
        {
            foreach (VisualElement screenContainer in m_screenContainers.Values)
            {
                foreach (VisualElement element in screenContainer.Children())
                {
                    if (element is T screen)
                    {
                        return screen;
                    }
                }
            }

            return default;
        }

        public IScreen GetScreen(Type type)
        {
            foreach (VisualElement screenContainer in m_screenContainers.Values)
            {
                foreach (VisualElement element in screenContainer.Children())
                {
                    if (type.IsAssignableFrom(element.GetType()))
                    {
                        return element as IScreen;
                    }
                }
            }

            return null;
        }
        
        public List<IScreen> GetAllScreens()
        {
            List<IScreen> screens = new();
            foreach (VisualElement screenContainer in m_screenContainers.Values)
            {
                foreach (VisualElement element in screenContainer.Children())
                {
                    if (element is IScreen baseScreen)
                    {
                        screens.Add(baseScreen);
                    }
                }
            }

            return screens;
        }

        // Will bring a selected screen to the front within it's ScreenType layer.
        public void BringScreenToFront(IScreen screen)
        {
            VisualElement screenElement = (VisualElement)screen;
            if (screenElement.parent == m_screenContainers[screen.ScreenType])
            {
                screenElement.BringToFront();
            }
        }
    }
}