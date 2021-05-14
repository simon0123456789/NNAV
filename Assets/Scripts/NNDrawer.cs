using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NNDrawer : MonoBehaviour
{
  
    public FitnessEvaluator fitnessEvaluator;
    public DistanceNNInterface distanceNNInterface;
    DistanceSensorArray sensors;

    public GameObject circleSprite;
    public float nodeScale = 5f;
    public Color negativeColor = Color.red;
    public Color positiveColor = Color.green;

    GameObject[][] layers;
    bool hasLoadedNNStructure = false;

    private void Start()
    {
        sensors = fitnessEvaluator.GetComponentInChildren<DistanceSensorArray>();
    }


    private void Update()
    {
        if (!hasLoadedNNStructure)
            PrepareNetworkStructure();

        if(distanceNNInterface.neuralNetwork != null)        
            DrawNetwork();
     }
    


    public void PrepareNetworkStructure()
    {

        var nn = distanceNNInterface.neuralNetwork;

        int numberOfLayers = nn.numberOfLayers;
        layers = new GameObject[numberOfLayers][];
        for (int i = 0; i < numberOfLayers; i++)
            layers[i] = DrawLayer(i, nn.layerSizes[i]);
        hasLoadedNNStructure = true;

    }


    GameObject[] DrawLayer(int layer, int size)
    {

        float yOffset = 20f;
        float xOffset = 50f;
        float height = yOffset * size;

        List<GameObject> nodes = new List<GameObject>();
        for (int i = 0; i < size; i++)
        {
            Vector3 position = new Vector3(xOffset * layer, yOffset * i - height / 2);
            nodes.Add(Instantiate(circleSprite, transform.position + position, Quaternion.identity, transform));

        }
        foreach (var n in nodes) n.transform.localScale = new Vector3(nodeScale, nodeScale, nodeScale);
        return nodes.ToArray();

    }

    public void DrawNetwork()
    {
        var nn = distanceNNInterface.neuralNetwork;
        for (int layer = 0; layer < layers.Length; layer++)
        {
            for (int row = 0; row < layers[layer].Length; row++)
            {
                float lerpValue;
                if (layer == 0) 
                    lerpValue = nn.inputLayer[row] / sensors.maxDistance;
                else if (layer == layers.Length - 1) 
                    lerpValue = (nn.outputLayer[row] + 1f) / 2f;
                else 
                    lerpValue = nn.hiddenLayers[layer - 1][row];

                layers[layer][row].GetComponent<Image>().color = Color.Lerp(negativeColor, positiveColor, lerpValue);
            }
        }

    }

}