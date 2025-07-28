using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private Deck deck;
    public bool rised = false;
    public bool hover =false;
    // Start is called before the first frame update
    void Start()
    {
        deck = FindObjectOfType<Deck>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInArray();
    }
    void CheckInArray()
    {
        if(deck.cardColliders != null)
        {
            for(int i = 0; i < deck.cardColliders.Length; i++)
            {
                if(this.name == deck.cardColliders[i].name)
                {
                    hover = true;
                }
                else
                {
                    hover=false;
                    if (rised)
                    {
                        deck.ScaleDown(this.transform);
                        rised = false;
                    }
                }
            }
        }
        if(deck.cardColliders == null)
        {
            Debug.Log("N");
            hover = false;
            if (rised)
            {
                deck.ScaleDown(this.transform);
                rised = false;
            }
        }
    }
}
