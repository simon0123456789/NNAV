using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Produces numberOfRays rays from transform.position spaced out evenly at fieldOfViewDeg/n degrees from forward direction.
/// Returns a float[numberOfRays] measuring the distances (m) to nearest solid object. 
/// </summary>
public class DistanceSensorArray : MonoBehaviour
{
   
    
    public int numberOfRays { get; private set; } = 5;

    [Range(5, 360)]
    public float fieldOfViewDeg = 90f;

    [Range(1, 300)]
    public float maxDistance = 40f;
    public bool fixMaxDistanceBug = false;

    public float[] outputs;



    Transform trans;


    public void Configure(int numberOfRays, float fieldOfViewDeg = -1f, float maxDistance = -1f) {
        this.numberOfRays = numberOfRays;     
        if(fieldOfViewDeg != -1f)
            this.fieldOfViewDeg = fieldOfViewDeg;     
        if(maxDistance != -1f)
            this.maxDistance = maxDistance;
        outputs = new float[numberOfRays];
    }

    private void Start() {
        trans = GetComponent<Transform>();
        Configure(numberOfRays, fieldOfViewDeg, maxDistance);
    }


    private void Update()
    {
    
        float angleDeltaDeg = fieldOfViewDeg / (numberOfRays - 1);
        float startAngle = -fieldOfViewDeg / 2f;

        for (int i = 0; i < numberOfRays; i++)
        {
            // we pick 2 arbitrary directions as basis for spherical interpolation. 
            // we choose forward and right
            float interpolationValue = (startAngle + angleDeltaDeg * i) / 90f;
            Vector3 direction = Vector3.SlerpUnclamped(trans.forward, trans.right, interpolationValue);

            Ray ray = new Ray(trans.position, direction);
            Vector3 endPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                outputs[i] = hit.distance;
                endPoint = hit.point;
            }
            else
            {
                endPoint = ray.origin + direction * maxDistance;
                if(fixMaxDistanceBug)
                    outputs[i] = maxDistance;
            }
            LineDrawer.DrawDistanceSensor(ray.origin, endPoint, i);
        }

     }
        
   
}
