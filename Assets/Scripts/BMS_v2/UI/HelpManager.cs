using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BMS_v2
{
    public class HelpManager : MonoBehaviour
    {
        public Button closeBtn;
        
        private void Start()
        {
            if (closeBtn != null) closeBtn.onClick.AddListener(CloseHelp);
            gameObject.SetActive(false);
        }

        public void OpenHelp()
        {
            gameObject.SetActive(true);
        }

        public void CloseHelp()
        {
            gameObject.SetActive(false);
        }
    }
}
