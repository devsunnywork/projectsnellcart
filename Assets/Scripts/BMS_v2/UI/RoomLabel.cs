using UnityEngine;
using TMPro;

namespace BMS_v2
{
    
    public class RoomLabel : MonoBehaviour
    {
        public TextMeshProUGUI roomNameText;

        public void SetRoomName(string name)
        {
            if (roomNameText != null) roomNameText.text = name;
        }
    }
}
