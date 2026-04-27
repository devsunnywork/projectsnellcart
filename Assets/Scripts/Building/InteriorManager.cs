using UnityEngine;

public class InteriorManager : MonoBehaviour
{
    // IS SCRIPT KI AB ZAROORAT NAHI HAI. 
    // SARA LOGIC AERIAL UI MANAGER MEIN MOVE HO GAYA HAI.
    // ISSE DELETE KAR DIYA JAYE TO BEHTER HAI.

    void Start()
    {
        Debug.LogWarning("InteriorManager redundant hai. AerialUIManager use karein.");
        this.enabled = false; // Script ko disable kar raha hoon taaki conflict na ho
    }
}
