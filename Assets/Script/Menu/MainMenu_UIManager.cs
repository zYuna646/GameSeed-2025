using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu_UIManager : MonoBehaviour
{
    public static MainMenu_UIManager Instance;
    public GameObject showLevel;
    public GameObject level1;
    public GameObject level2;
    public GameObject level3;
    private void Awake()
    {
        Instance = this;
    }

}
