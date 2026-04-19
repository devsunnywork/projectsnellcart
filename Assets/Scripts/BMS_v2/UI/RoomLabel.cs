using UnityEngine;
using TMPro;

namespace BMS_v2
{
    /// <summary>
    /// Simple script to hold referencing to the TextMeshPro component of a 3D Room Label.
    /// Used by the BuildingLabelGenerator to spawn and update text at runtime.
    /// </summary>
    public class RoomLabel : MonoBehaviour
    {
        public TextMeshProUGUI roomNameText;

        public void SetRoomName(string name)
        {
            if (roomNameText != null) roomNameText.text = name;
        }
    }
}
