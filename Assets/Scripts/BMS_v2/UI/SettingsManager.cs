using UnityEngine;
using UnityEngine.UI;

namespace BMS_v2
{
    public class SettingsManager : MonoBehaviour
    {
        public Button resumeBtn;
        public Button exitBtn;



        private void Start()
        {
            if (resumeBtn != null) resumeBtn.onClick.AddListener(CloseSettings);
            if (exitBtn != null) exitBtn.onClick.AddListener(ExitApp);

            
            gameObject.SetActive(false);
        }

        public void OpenSettings()
        {
            gameObject.SetActive(true);
        }

        public void CloseSettings()
        {
            gameObject.SetActive(false);
        }

        private void ExitApp()
        {
            Debug.Log("Exiting System...");
            Application.Quit();
        }
    }
}
