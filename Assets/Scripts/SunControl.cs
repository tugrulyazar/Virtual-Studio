using UnityEngine;
using UnityEngine.UI;

namespace UserBehaviour
{
    public class SunControl : MonoBehaviour
    {
        [SerializeField]
        private PlayerController playerController;
        [SerializeField]
        private GameObject sunMenu;

        [SerializeField]
        private GameObject sun;

        [SerializeField]
        private Slider slider1;
        [SerializeField]
        private Slider slider2;

        private bool menuActive;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.End))
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

            sunMenu.SetActive(true);
        }

        private void deactivateMenu()
        {
            menuActive = false;
            playerController.input.Enable();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            sunMenu.SetActive(false);
        }

        private void OnEnable()
        {
            slider1.value = sun.transform.rotation.eulerAngles.x;
            slider2.value = sun.transform.rotation.eulerAngles.y;
        }

        public void adjustSun()
        {
            sun.transform.rotation = Quaternion.Euler(slider1.value, slider2.value, 0);
        }
    }
}

