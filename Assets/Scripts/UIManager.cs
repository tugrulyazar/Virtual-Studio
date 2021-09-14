using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private UserBehaviour.PlayerController playerController;

    public bool menuActive;

    public void activateMenu()
    {
        menuActive = true;
        playerController.input.Disable();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void deactivateMenu()
    {
        menuActive = false;
        playerController.input.Enable();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

}
