using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public GameObject playPanel;
    public GameObject bookchesspanel;
    public GameObject bookcardpanel;

    public GameObject Pion;
    public GameObject Bishop;
    public GameObject Kuda;
    public GameObject Benteng;
    public GameObject Ratu;
    public GameObject Raja;

    public GameObject card;
    public GameObject rarity;
    public GameObject legendary;
    public GameObject rare;
    public GameObject common;
    public GameObject atk;
    public GameObject def;
    public GameObject buff;

    public void OnPlayPressed() { playPanel.SetActive(true); }
    public void OnHostPressed() { Debug.Log("Host Game Clicked"); }
    public void OnJoinPressed() { Debug.Log("Join Game Clicked"); }
    public void OnBackFromPlay()
    {
        if (playPanel.activeSelf == true)
        {
            playPanel.SetActive(false);

        }
        else if (bookchesspanel.activeSelf == true)
        {
            bookchesspanel.SetActive(false);
        }
        else if (bookcardpanel.activeSelf == true)
        {
            bookcardpanel.SetActive(false);
        }
    }
    public void OnInformasiPressed() { Debug.Log("Menampilkan informasi atau credit."); }
    public void OnMenuCardPressed() { bookcardpanel.SetActive(true); }
    public void OnnextPressed()
    {
        if (Pion.activeSelf == true)
        {
            Pion.SetActive(false);
            Bishop.SetActive(true);

            card.SetActive(false);
            rarity.SetActive(true);
        }
        else if (Bishop.activeSelf == true)
        {
            Bishop.SetActive(false);
            Kuda.SetActive(true);

            rarity.SetActive(false);
            legendary.SetActive(true);
        }
        else if (Kuda.activeSelf == true)
        {
            Kuda.SetActive(false);
            Benteng.SetActive(true);
        }
        else if (Benteng.activeSelf == true)
        {
            Benteng.SetActive(false);
            Ratu.SetActive(true);

        }
        else if (Ratu.activeSelf == true)
        {
            Ratu.SetActive(false);
            Raja.SetActive(true);
        }
    }
    public void OnpreviousPressed()
    {
        if (Raja.activeSelf == true)
        {
            Raja.SetActive(false);
            Ratu.SetActive(true);
        }
        else if (Ratu.activeSelf == true)
        {
            Ratu.SetActive(false);
            Benteng.SetActive(true);
        }
        else if (Benteng.activeSelf == true)
        {
            Benteng.SetActive(false);
            Kuda.SetActive(true);
        }
        else if (Kuda.activeSelf == true)
        {
            Kuda.SetActive(false);
            Bishop.SetActive(true);
        }
        else if (Bishop.activeSelf == true)
        {
            Bishop.SetActive(false);
            Pion.SetActive(true);
        }
    }


    public void OnnextcardPressed()
    {
        if (card.activeSelf == true)
        {
            card.SetActive(false);
            rarity.SetActive(true);
        }
        else if (rarity.activeSelf == true)
        {
            rarity.SetActive(false);
            legendary.SetActive(true);
        }
        else if (legendary.activeSelf == true)
        {
            legendary.SetActive(false);
            rare.SetActive(true);
        }
        else if (rare.activeSelf == true)
        {
            rare.SetActive(false);
            common.SetActive(true);
        }
        else if (common.activeSelf == true)
        {
            common.SetActive(false);
            atk.SetActive(true);
        }
        else if (atk.activeSelf == true)
        {
            atk.SetActive(false);
            def.SetActive(true);
        }
        else if (def.activeSelf == true)
        {
            def.SetActive(false);
            buff.SetActive(true);
        }
    }
    public void OnpreviouscardPressed()
    {
        if (buff.activeSelf == true)
        {
            buff.SetActive(false);
            def.SetActive(true);
        }
        else if (def.activeSelf == true)
        {
            def.SetActive(false);
            atk.SetActive(true);
        }
        else if (atk.activeSelf == true)
        {
            atk.SetActive(false);
            common.SetActive(true);
        }
        else if (common.activeSelf == true)
        {
            common.SetActive(false);
            rare.SetActive(true);
        }
        else if (rare.activeSelf == true)
        {
            rare.SetActive(false);
            legendary.SetActive(true);
        }
        else if (legendary.activeSelf == true)
        {
            legendary.SetActive(false);
            rarity.SetActive(true);
        }
        else if (rarity.activeSelf == true)
        {
            rarity.SetActive(false);
            card.SetActive(true);
        }
    }
    public void OnMenuChessPressed() { bookchesspanel.SetActive(true); }


}
