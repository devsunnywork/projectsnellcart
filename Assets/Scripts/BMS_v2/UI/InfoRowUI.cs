using UnityEngine;
using TMPro;

namespace BMS_v2
{
    /// <summary>
    /// UI component representing a single row of label-value data in the Asset Detail panel.
    /// Allows toggling an input field to edit the value.
    /// </summary>
    public class InfoRowUI : MonoBehaviour
    {
        public TextMeshProUGUI labelText;
        public TextMeshProUGUI valueText;
        public TMP_InputField valueInput; 
        
        private System.Action<string> onSaveAction;

        public void Setup(string label, string value, System.Action<string> onSave = null)
        {
            if (labelText != null) labelText.text = label;
            if (valueText != null) valueText.text = value;
            
            if (valueInput != null)
            {
                valueInput.text = value;
                valueInput.gameObject.SetActive(false); 
            }
            if (valueText != null) valueText.gameObject.SetActive(true);

            onSaveAction = onSave;
        }

        public void SetEditMode(bool isEditing)
        {
            
            if (valueInput == null || onSaveAction == null) return;

            if (isEditing)
            {
                valueText.gameObject.SetActive(false);
                valueInput.gameObject.SetActive(true);
            }
            else
            {
                
                string newVal = valueInput.text;
                valueText.text = newVal;
                valueInput.gameObject.SetActive(false);
                valueText.gameObject.SetActive(true);

                
                onSaveAction?.Invoke(newVal);
            }
        }
    }
}
