using UnityEngine;

namespace UserBehaviour
{
    public class EscMenu : MonoBehaviour
    {
        [SerializeField]
        private PlayerController playerController;
        [SerializeField]
        private GameObject escMenu;

        private bool menuActive;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!menuActive)
                {
                    activateMenu();
                }
                else                
                {
                    deactivateMenu();
                }

            }
        }

        private void activateMenu()
        {
            menuActive = true;
            playerController.input.Disable();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            escMenu.SetActive(true);
        }

        private void deactivateMenu()
        {
            menuActive = false;
            playerController.input.Enable();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            escMenu.SetActive(false);
        }

        public void exitGame()
        {
            Application.Quit();
            Debug.Log("Exit Game");
        }
    }
}

