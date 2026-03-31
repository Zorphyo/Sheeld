using UnityEngine;

public class InteractPopup : MonoBehaviour
{
    public GameObject interactPopUp;

    void Start()
    {
        DisablePopUp();
    }
    
    public void EnablePopUp()
    {
        interactPopUp.SetActive(true);
    }

    public void DisablePopUp()
    {
        interactPopUp.SetActive(false);
    }
}
