using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UICoreSystem
{
    public class PrimaryLayoutSpawner : MonoBehaviour
    {
        [SerializeField]
        private PanelRenderer m_panelRenderer;
        private TaskCompletionSource<PrimaryLayout> m_primaryLayoutReady = new();
        public Task<PrimaryLayout> PrimaryLayoutReady => m_primaryLayoutReady.Task;

        public void Awake()
        {
            _ = FocusPrimaryLayout();
        }

        private void OnEnable()
        {
            m_panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDisable()
        {
            m_panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        }

        // Also runs on re-enable and live reload. Only the first call matters.
        private void OnUIReload(PanelRenderer panelRenderer, VisualElement root, int version)
        {
            m_primaryLayoutReady.TrySetResult(root.Q<PrimaryLayout>());
        }

        /// <summary>
        /// Technically this should only take one frame to wait for PanelSettings GO to be created. But we'll loop and keep
        /// checking just to be safe.
        /// </summary>
        private async Task FocusPrimaryLayout()
        {
            PanelEventHandler panelEventHandler = null;
            while (panelEventHandler == null)
            {
                await Task.Delay(20);
                if (this == null)
                {
                    return;
                }
                panelEventHandler = FindAnyObjectByType<PanelEventHandler>();
            }

            EventSystem.current.SetSelectedGameObject(panelEventHandler.gameObject);
        }
    }
}
