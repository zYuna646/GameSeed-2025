using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Deck : MonoBehaviour
{
    

    [SerializeField] int deckLimit = 0;
    [SerializeField] public Collider[] cardColliders;
    [SerializeField] public LayerMask cardMask;
    public List<GameObject> cards;
    bool state = false;
    

    Vector3 cameraPoint;
    [SerializeField] float cameraOffset;
    [SerializeField] Transform pile;
    [SerializeField] Transform spreadPoint;
    
    Vector3 mousePos;
    private Coroutine spreadRoutine;

    
    private void Awake()
    {
        
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
        if (Input.GetMouseButtonDown(0))
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
                    100f * Time.deltaTime
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
    }

    void ScaleUp(Transform card)
    {
        

        float rise = 0.9f;
        card.position = Vector3.MoveTowards(card.position, card.position + new Vector3(0, rise, 0), 100f * Time.deltaTime);
        card.GetComponent<Card>().rised = true;
    }

    public void ScaleDown(Transform card)
    {
        float down = 0.9f;
        card.position = Vector3.MoveTowards(card.position, card.position - new Vector3(0, down, 0), 100f * Time.deltaTime);
    }

    public List<GameObject> FindObjectsInLayer(LayerMask layer)
    {

        List<GameObject> foundObjects = new List<GameObject>();
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject go in allObjects)
        {
            if ((layer.value & (1 << go.layer)) != 0)
            {
                foundObjects.Add(go);
            }
        }
        return foundObjects;
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