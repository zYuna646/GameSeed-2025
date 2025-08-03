using UnityEngine;
using UnityEngine.UI;

public class ClickAnywhere : MonoBehaviour
{
    public GameObject menuButtons;

    void Start()
    {
        menuButtons.SetActive(false); // Sembunyikan tombol di awal
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Klik kiri
        {
            menuButtons.SetActive(true); // Tampilkan tombol
        }
    }
}
