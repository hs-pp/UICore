using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UICoreSystem
{
    public class PrimaryLayoutSpawner : MonoBehaviour
    {
        [SerializeField]
        private UIDocument m_document;
        private PrimaryLayout m_primaryLayout;
        public PrimaryLayout PrimaryLayout => m_primaryLayout;

        public void Awake()
        {
            m_primaryLayout = m_document.rootVisualElement.Q<PrimaryLayout>();
            _ = FocusPrimaryLayout();
        }
        
        /// <summary>
        /// Technically this should only take one frame to wait for PanelSettings GO to be created. But we'll loop and keep
        /// checking just to be safe.
        /// </summary>
        private async Task FocusPrimaryLayout() // TODO: Convert this to coroutine to avoid leaks!
        {
            PanelEventHandler panelEventHandler = null;
            while (panelEventHandler == null)
            {
                await Task.Delay(20);
                panelEventHandler = FindAnyObjectByType<PanelEventHandler>();
            }

            EventSystem.current.SetSelectedGameObject(panelEventHandler.gameObject);
        }
    }
}