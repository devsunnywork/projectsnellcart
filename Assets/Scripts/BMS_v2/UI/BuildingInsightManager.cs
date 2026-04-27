using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BMS_v2
{
    /// <summary>
    /// Manages the Building Insight Table panel, which shows high-level metrics
    /// when a building is selected in the front view.
    /// </summary>
    public class BuildingInsightManager : MonoBehaviour
    {
        public static BuildingInsightManager Instance;

        [Header("UI References")]
        public GameObject panel;
        public Button closeButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HidePanel);
            }
        }

        public void ShowPanel()
        {
            if (panel != null)
            {
                panel.SetActive(true);
                // Bring to front
                panel.transform.SetAsLastSibling();
            }
        }

        public void HidePanel()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
