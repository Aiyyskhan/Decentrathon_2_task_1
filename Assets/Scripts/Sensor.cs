using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sensor : MonoBehaviour
{
    public const int numRays = 5;
    public const float maxDistance = 20.0f;
    [SerializeField] LayerMask collisionLayerMask;
    private Ray[] rays = new Ray[numRays];
    public float[] distances = new float[numRays];
    private int count = 0;

    void Awake()
    {
        for (int i = 0; i < numRays; i++)
        {
            distances[i] = 0.0f;
        }
    }

    void Start()
    {
        RayCreate();
    }

    void FixedUpdate()
    {   
        RayCreate();
        RayCasting();
    }

    void RayCreate()
    {
        float gap = Mathf.PI / (numRays - 1.0f);
        for (int i = 0; i < numRays; i++)
        {   
            float ang = gap * (float)i + transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(
                Mathf.Cos(ang), 
                0.0f, 
                -Mathf.Sin(ang)
            );
            rays[i] = new Ray(transform.position, dir);
        }
    }

    void RayCasting()
    {
        for (int i = 0; i < numRays; i++)
        {
            bool collision = Physics.Raycast(rays[i], out RaycastHit hit, maxDistance, collisionLayerMask);
            float dist = collision ? hit.distance : maxDistance;
            distances[i] = dist / maxDistance;

            Debug.DrawRay(rays[i].origin, rays[i].direction * dist, collision ? Color.green : Color.red);
        }
    }
}
