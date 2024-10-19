using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointContainer : MonoBehaviour
{
    public List<Transform> waypoints;
    void Awake()
    {
        foreach (Transform item in gameObject.GetComponentsInChildren<Transform>())
        {
            waypoints.Add(item);
        }
        waypoints.Remove(waypoints[0]);
    }

    void Update()
    {
        
    }
}
