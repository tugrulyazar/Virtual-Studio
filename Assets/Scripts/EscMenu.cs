using UnityEngine;

public class EscMenu : MonoBehaviour
{
    [SerializeField]
    private UIManager ui;
    [SerializeField]
    private UserBehaviour.PlayerController playerController;
    [SerializeField]
    private GameObject escMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!ui.menuActive)
            {
                ui.activateMenu();
                escMenu.SetActive(true);
            }
            else if (escMenu.activeSelf)
            {
                ui.deactivateMenu();
                escMenu.SetActive(false);
            }

        }
    }

    public void exitGame()
    {
        Application.Quit();
        Debug.Log("Exit Game");
    }
}