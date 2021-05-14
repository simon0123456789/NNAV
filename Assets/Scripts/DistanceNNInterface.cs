using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceNNInterface : MonoBehaviour
{


    [Range(-1f, 1f)]
    public float steering = 0f;

    [Range(-1f, 1f)]
    public float acceleration = 0f;

    


    public enum InputMode { distance, line, combo}   
    public InputMode inputMode { get; private set; } = InputMode.distance;
    public float[] inputs;


    public enum OutputMode { neural, algorithmic}
    public OutputMode outputMode = OutputMode.neural;

    [Range(0.01f, 10)]
    public float algorithmicSensitivity = 1f;
    [Range(-1, 1)]
    public float algorithmicSteering = 0;


    [HideInInspector]
    public NeuralNetwork neuralNetwork;
    [HideInInspector]
    public DistanceSensorArray distanceSensorArray;
    [HideInInspector]
    public VideoInput videoInput;



    public void SetupInputs(InputMode inputMode, int numberOfRays = 0) {

        this.inputMode = inputMode;

        switch (inputMode) {
            case InputMode.distance:          
                distanceSensorArray.gameObject.SetActive(true);
                videoInput.gameObject.SetActive(false);
                distanceSensorArray.Configure(numberOfRays);
                inputs = distanceSensorArray.outputs;
                break;
            case InputMode.line:
                distanceSensorArray.gameObject.SetActive(false);
                videoInput.gameObject.SetActive(true);
                inputs = new float[2];
                break;
            case InputMode.combo:
                distanceSensorArray.gameObject.SetActive(true);
                distanceSensorArray.Configure(numberOfRays);
                videoInput.gameObject.SetActive(true);
                inputs = new float[2 + distanceSensorArray.outputs.Length];
                break;
        }

    }


    void Start()
    {
        distanceSensorArray = GetComponentInChildren<DistanceSensorArray>();
        videoInput = GetComponentInChildren<VideoInput>();
        SetupInputs(inputMode, distanceSensorArray.outputs.Length);

    }


    void Update() {

        steering = 0f;
        acceleration = 0f;



        switch (inputMode) {
            case InputMode.distance:
                // just reads directly from distance sensor
                break;
            case InputMode.line:
                inputs[0] = videoInput.leftTilt;
                inputs[1] = videoInput.rightTilt;
                break;
            case InputMode.combo:
                inputs[0] = videoInput.leftTilt;
                inputs[1] = videoInput.rightTilt;
                Array.Copy(distanceSensorArray.outputs, 0, inputs, 2, distanceSensorArray.outputs.Length);
                break;
        }


        algorithmicSteering = 0;     
        float tiltSum = videoInput.leftTilt + videoInput.rightTilt;
        algorithmicSteering = -tiltSum * algorithmicSensitivity;
        if (float.IsNaN(algorithmicSteering))
            algorithmicSteering = 0;
        algorithmicSteering = Mathf.Clamp(algorithmicSteering, -1, 1);
        


        switch (outputMode) {
            case OutputMode.neural:
                float[] outputs = neuralNetwork.FeedForward(inputs);
                acceleration = outputs[0];
                steering = outputs[1];
                break;
            case OutputMode.algorithmic:
                steering = algorithmicSteering;
                break;
        }



        if (float.IsNaN(acceleration))
            acceleration = 0;
        acceleration = Mathf.Clamp(acceleration, -1, 1);

        if (float.IsNaN(steering))
            steering = 0;
        steering = Mathf.Clamp(steering, -1, 1);


    }
}
