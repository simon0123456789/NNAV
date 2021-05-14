using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wrapper for Unity's CarController script. Accepts steering and acceleration input from either neural network or manual input. 
/// </summary>
public class CarInput : MonoBehaviour
{


    [Range(-1f, 1f)]
    public float steering;
    
    [Range(-1f, 1f)]
    public float acceleration;

    [Range(1, 50)]
    public float brakeModifier = 10f;



    public bool autoForward = false;
    public bool useNNInterface = true;



    [HideInInspector]
    public DistanceNNInterface distanceNNInterface;
    UnityStandardAssets.Vehicles.Car.CarController carController;



    private void Start()
    {
        carController = GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();
        distanceNNInterface = GetComponent<DistanceNNInterface>();

    }


    private void Update()
    {

        steering = 0f;
        acceleration = 0f;


        if (autoForward)
            acceleration += 1f;



        steering += SimpleInput.GetAxis("Horizontal");      
        if(Input.GetJoystickNames().Length > 0)
            acceleration += Input.GetAxis("BothTriggers");
        else
            acceleration += SimpleInput.GetAxis("Vertical");



        if(useNNInterface) {           
            acceleration += distanceNNInterface.acceleration;
            steering += distanceNNInterface.steering;            
        }
     
 
        acceleration = Mathf.Clamp(acceleration, -1, 1);
        steering = Mathf.Clamp(steering, -1, 1);

    }


    private void FixedUpdate()
    {
        float accel     = acceleration > 0 ? acceleration : 0;    // [ 0, 1]
        float footbrake = acceleration < 0 ? acceleration : 0;    // [-1, 0]

        carController.Move(steering, accel, footbrake * brakeModifier, 0);
    }


}
