using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public class Deck : MonoBehaviour
{
    private GameManager gameManager;
    
    [SerializeField] public Collider[] cardColliders;
    [SerializeField] public LayerMask cardMask;
    public List<GameObject> cards;
    bool state = false;
    

    Vector3 cameraPoint;
    [SerializeField] float cameraOffset;
    [SerializeField] Transform pile;
    [SerializeField] Transform spreadPoint;
    [SerializeField] Transform deckPoint;
    [SerializeField] int deckCounter = 0;
    
    Vector3 mousePos;
    private Coroutine spreadRoutine;
    private Coroutine arangeRoutine;

    
    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameManager.currentDeckPoint = 0;
    }

    void Start()
    {
        cards = FindObjectsInLayer(cardMask);
        StackCard();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SortingListByName();
        }
        if (Input.GetKeyDown(KeyCode.P) && state != true)
        {
            if (spreadRoutine != null)
                StopCoroutine(spreadRoutine);
            spreadRoutine = StartCoroutine(SpreadCard(spreadPoint));
        }
        mousePos = Input.mousePosition;
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z = cameraOffset;
        
        
        Hover();
        if (Input.GetMouseButtonDown(0) && cardColliders.Length != 0)
        {
            PickCard();
        }

        //CHECKBUG(cardMask, gameobj);
    }

    //void CHECKBUG(LayerMask layer, GameObject go)
    //{
    //    if (go.layer == (Mathf.Log(layer.value) / Mathf.Log(2)))
    //    {
    //        Debug.Log("Sama");
    //    }
    //    else
    //    {
    //        Debug.Log(go.layer);
    //        Debug.Log(layer.value);
    //    }
    //}

    IEnumerator SpreadCard(Transform spreadPoint)
    {
        state = true;
        Vector3 originalSpreadPoint = spreadPoint.position; // Cache original position

        float cardLength = 0;
        int counter = 0;
        int cardsPerRow = 9;

        foreach (GameObject card in cards)
        {
            Vector3 targetPosition;
            if (counter < cardsPerRow)
            {
                targetPosition = originalSpreadPoint + new Vector3(cardLength, 0, 0);
                cardLength += 3.3f;
            }
            else
            {
                originalSpreadPoint += new Vector3(0, 0, -6.61f);
                counter = 0;
                cardLength = 3.3f;
                targetPosition = originalSpreadPoint;
            }

            while (Vector3.Distance(card.transform.position, targetPosition) > 0.01f)
            {
                card.transform.position = Vector3.MoveTowards(
                    card.transform.position,
                    targetPosition,
                    300f * Time.deltaTime
                );
                yield return null;
            }


            card.transform.position = targetPosition;
            counter++;
        }

        state = false;
    }
    

    void StackCard()
    {
        float counter = 0.03f;
        Vector3 stack = new Vector3(0, 0 ,0);
        foreach (GameObject card in cards)
        {
            card.transform.position = pile.position + stack;
            stack = stack + new Vector3(0, counter ,0);
            
        }
    }

    void SortingListByName()
    {
        cards.Sort((a,b) => a.name.CompareTo(b.name));
        StackCard();
    }

    void SortingListByPower()
    {
        
    }
    void Hover()
    {
        cameraPoint = Camera.main.ScreenToWorldPoint(mousePos);
        cardColliders = Physics.OverlapCapsule(transform.position, cameraPoint, 0.1f, cardMask);
        foreach (Collider card in cardColliders)
        {
            if (card.GetComponent<Card>().hover && !card.GetComponent<Card>().rised)
            {
                ScaleUp(card.transform);
            }
            
        }
    }

    //void CardInfo(GameObject card)
    //{
    //    card.transform.position
    //}

    void PickCard()
    {
        if(cardColliders.Length == 1)
        {
            CheckPoint(cardColliders[0].GetComponent<Card>());
        }
        else if(cardColliders.Length > 1)
        {
            CheckPoint(cardColliders[cardColliders.Length - 1].GetComponent<Card>());
        }
        //for(int i = 0; i < cardColliders.Length; i++)
        //{
        //    //cardColliders[i].GetComponent<Card>().originalDeckPos = cardColliders[i].transform.position;
            
        //}
    }

    void ScaleUp(Transform card)
    {
        

        
        Vector3 rise = new Vector3(card.position.x, 1.3f, card.position.z);
        card.position = Vector3.MoveTowards(card.position, rise, 100f * Time.deltaTime);
        card.localScale = Vector3.Lerp( card.localScale, new Vector3(0.39f, 0.686052f, 0.5574172f), 200f * Time.deltaTime);
        card.GetComponent<Card>().rised = true;
    }

    public void ScaleDown(Transform card)
    {
        Vector3 down = new Vector3(card.position.x, 0.4f, card.position.z);
        card.position = Vector3.MoveTowards(card.position, down, 100f * Time.deltaTime);
        card.localScale = Vector3.Lerp(card.localScale, new Vector3(0.27f, 0.4749591f, 0.3859043f), 200f * Time.deltaTime);
    }

    public List<GameObject> FindObjectsInLayer(LayerMask layer)
    {

        List<GameObject> foundObjects = new List<GameObject>();
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if ((layer.value & (1 << go.layer)) != 0)
            {
                foundObjects.Add(go);
            }
        }
        return foundObjects;
    }

    void CheckPoint(Card card)
    {
        if(card.cardPoint + gameManager.currentDeckPoint <= gameManager.deckPoint && !card.inDeck)
        {
                AddToDeck(card);
                gameManager.currentDeckPoint += card.cardPoint;
        }
        else if(card.inDeck)
        {
            RemoveFromDeck(card);
            gameManager.currentDeckPoint -= card.cardPoint;
        }
    }

    void AddToDeck(Card card)
    {
        GameObject[] tempArray = new GameObject[gameManager.decks.Length + 1];
        for (int i = 0; i < gameManager.decks.Length; i++)
        {
            tempArray[i] = gameManager.decks[i];
        }
        tempArray[tempArray.Length - 1] = card.gameObject;
        gameManager.decks = tempArray;
        card.inDeck = true;

        if (spreadRoutine != null)
            StopCoroutine(spreadRoutine);
        spreadRoutine = StartCoroutine(ToDeck(deckPoint, card));
    }

    void RemoveFromDeck(Card card)
    {

        List<GameObject> deckList = new List<GameObject>(gameManager.decks);

        GameObject cardToRemove = deckList.Find(go => go.name == card.name);
        if (cardToRemove != null)
        {
            deckList.Remove(cardToRemove);
            gameManager.decks = deckList.ToArray();
            card.inDeck = false;
        }

        if (spreadRoutine != null)
            StopCoroutine(spreadRoutine);
        spreadRoutine = StartCoroutine(RemoveDeck(card));
        
    }

    void ReArangeDeck(Transform deckPos)
    {
        int counter = 0;
        for(int i = 0;i < gameManager.decks.Length;i++)
        {
            if (arangeRoutine != null)
                StopCoroutine(arangeRoutine);
            arangeRoutine = StartCoroutine(ReArange(gameManager.decks[i], deckPos, counter));
            counter++;
        }
    }

    IEnumerator ReArange(GameObject go, Transform deckPos, int counter)
    {
        while (Vector3.Distance(go.transform.position, deckPos.position + new Vector3(3.3f * counter, 0, 0)) > 0.01f)
        {
            go.transform.position = Vector3.MoveTowards(go.transform.position, deckPos.position + new Vector3(3.3f * counter, 0, 0), 100f * Time.deltaTime);
            yield return null;
        }
        
    }

    IEnumerator ToDeck(Transform deckPos, Card card)
    {
        card.originalDeckPos = card.transform.position - new Vector3(0, 0.4f,0);
        Vector3 array = new Vector3(3.3f * deckCounter, 0, 0);

        while (Vector3.Distance(card.transform.position, deckPos.position +array) > 0.01f)
        {
            card.transform.position = Vector3.MoveTowards(card.transform.position, deckPos.position + array, 100f * Time.deltaTime);
            yield return null;
        }
        deckCounter++;
        
    }

    IEnumerator RemoveDeck( Card card)
    {
        while(Vector3.Distance(card.transform.position, card.originalDeckPos) > 0.01f)
        {
            card.transform.position = Vector3.MoveTowards(card.transform.position, card.originalDeckPos, 100f * Time.deltaTime);
            ReArangeDeck(deckPoint);
            yield return null;
        }
        deckCounter--;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, cameraPoint);
    }
}
//}IEnumerator SpreadCard(Transform spreadPoint)
//{
//    state = true;
//    float cardLength = 0;
//    int counter = 0;
//    foreach (GameObject card in cards)
//    {
//        if(counter <= 9)
//        {
//            card.transform.position = Vector3.MoveTowards(card.transform.position, spreadPoint.position + new Vector3(cardLength, 0, 0), 0.5f);
//            cardLength += 3.3f;
//        }
//        else
//        {
//            spreadPoint.position = spreadPoint.position + new Vector3(0, -6.61f, 0);
//            counter = 0;
//            cardLength = 0;
//        }

//        counter++;
//    }
//    state = false;
//}
//void RemoveFromDeck(Card card)
//{
//    Debug.Log("removing");
//    GameObject[] tempArray = new GameObject[gameManager.decks.Length - 1];
//    for(int i=0; i < gameManager.decks.Length; i++)
//    {
//        if(card.name == gameManager.decks[i].gameObject.name)
//        {
//            gameManager.decks[i] = null;
//        }
//    }


//    for(int i =0; i < tempArray.Length;)
//    {
//        int j = 0;
//        for(int k = 0; k< gameManager.decks.Length;)
//        {
//            k = j;
//            if (gameManager.decks[k] != null)
//            {
//                tempArray[i] = gameManager.decks[k].gameObject;
//                k = gameManager.decks.Length;
//                j++;
//                i++;
//            }
//            else
//            {
//                j++;
//            }
//        }

//    }
//    gameManager.decks = tempArray;
//    card.inDeck = false;

//    if (spreadRoutine != null)
//        StopCoroutine(spreadRoutine);
//    spreadRoutine = StartCoroutine(RemoveDeck( card));
//}