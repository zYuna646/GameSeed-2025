using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Ray_cam : MonoBehaviour
{
    [SerializeField] public float3 top;
    [SerializeField] public float3 bottom;
    [SerializeField] float3 topCam;
    [SerializeField] float3 bottomCam;
    [SerializeField] float3 difference;
    [SerializeField] float offset;
    // Start is called before the first frame update
    void Start()
    {
        GetTopPos();
        GetBotPos();
        difference = GetBotPos() - GetTopPos();
        
    }

    // Update is called once per frame
    void Update()
    {
        GetBotPos();
        top = CheckDifference(bottom, top, difference);
    }

    float3 CheckDifference(float3 bottom, float3 top, float3 diff)
    {
        float3 a = new float3 (0, 0 ,0);
        if (math.all(bottom - top != diff))
        {
            a = bottom - diff;
            return a;
        }
        else
        {
            return top;
        }
        
    }

    float3 GetTopPos()
    {
        topCam = Input.mousePosition;
        transform.position = Camera.main.ScreenToWorldPoint(topCam);
        top = transform.position;
        return top;

        
    }

    float3 GetBotPos()
    {
        bottomCam = new float3 (Input.mousePosition) + new float3(0, 0, offset);
        transform.position = Camera.main.ScreenToWorldPoint(bottomCam);
        bottom = transform.position;
        return bottom;
    }
}
