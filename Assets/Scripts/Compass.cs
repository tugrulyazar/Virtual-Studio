using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    [SerializeField]
    private UIManager ui;
    [SerializeField]
    private GameObject compassMenu;

    [SerializeField]
    private Slider compassSlider;

    private Transform mainCamera;
    private float correctionValue;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(0, 0, mainCamera.localRotation.eulerAngles.y - correctionValue);

        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (!ui.menuActive)
            {
                ui.activateMenu();
                compassMenu.SetActive(true);
            }
            else if (compassMenu.activeSelf)
            {
                ui.deactivateMenu();
                compassMenu.SetActive(false);
            }
        }
    }

    public void adjustCompass()
    {
        correctionValue = compassSlider.value;
    }
}
