using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointTracker : MonoBehaviour
{

    public Transform trackingTarget;

    public float totalDistanceTravelled = 0f;
    float distanceTravelledToNext       = 0f;   // our current progress from last waypoint toward next waypoint
    float stashedDistance               = 0f;   // the distances of the waypoints we've already covered


    public Transform waypointsParent;
    Transform[] waypoints;               
    Transform currentWaypoint;
    Transform nextWaypoint;
    Transform finalWaypoint;
    int currentWaypointIndex = 0;
    Vector3 vectorCurrentToNext;        




    // Start is called before the first frame update
    void Start()
    {
        // setup waypoint array from waypoint parent
        waypoints = new Transform[waypointsParent.childCount];
        for (int i = 0; i < waypointsParent.childCount; i++)
            waypoints[i] = waypointsParent.GetChild(i);
        ResetWaypoints();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 vectorCurrentToTrans = trackingTarget.position - currentWaypoint.position;
        
        distanceTravelledToNext = Vector3.Project(vectorCurrentToTrans, vectorCurrentToNext).magnitude;
        if (Vector3.Distance(trackingTarget.position, nextWaypoint.position) > vectorCurrentToNext.magnitude)
            distanceTravelledToNext *= -1;
        
        totalDistanceTravelled = stashedDistance + distanceTravelledToNext;
    }

    void ResetWaypoints()
    {
        currentWaypointIndex = 0;
        currentWaypoint = waypoints[currentWaypointIndex];
        nextWaypoint = waypoints[currentWaypointIndex + 1];
        finalWaypoint = waypoints[waypoints.Length - 1];

        vectorCurrentToNext = nextWaypoint.position - currentWaypoint.position;

    }

    void UpdateWaypoints()
    {
        stashedDistance += vectorCurrentToNext.magnitude;
        currentWaypointIndex++;
        currentWaypoint = nextWaypoint;

        if (currentWaypoint == finalWaypoint)
        {
            Debug.Log("final waypoint reached");
        }

        nextWaypoint = waypoints[currentWaypointIndex + 1];
        vectorCurrentToNext = nextWaypoint.position - currentWaypoint.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == waypoints[currentWaypointIndex + 1].gameObject)
            UpdateWaypoints();

    }


}
