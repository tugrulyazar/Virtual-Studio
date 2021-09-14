using UnityEngine;
using UnityEngine.UI;

public class SunControl : MonoBehaviour
{
    [SerializeField]
    private UIManager ui;
    [SerializeField]
    private GameObject sunMenu;
    [SerializeField]
    private GameObject sun;

    [SerializeField]
    private Slider slider1;
    [SerializeField]
    private Slider slider2;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!ui.menuActive)
            {
                ui.activateMenu();
                sunMenu.SetActive(true);
            }
            else if (sunMenu.activeSelf)
            {
                ui.deactivateMenu();
                sunMenu.SetActive(false);
            }
        }
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