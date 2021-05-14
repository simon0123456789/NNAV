using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class NeuralNetwork : IComparable
{

    public Vector<float>[] activations;
    public Matrix<float>[] weights;
    public Vector<float>[] weightedInputs;
    public Vector<float>[] biases;

    public int numberOfLayers;
    public int[] layerSizes;


    // for GD & BP
    public bool useGuassian = false;
    public bool useSigmoid = false;
    public bool useBiases = false;


    // legacy access
    public Vector<float> inputLayer {
        get {
            return activations[0];
        }
    }
    public Vector<float> outputLayer {
        get {
            return activations[numberOfLayers - 1];
        }
    }
    public Vector<float>[] hiddenLayers {
        get {
            return activations.Skip(1).Take(activations.Length - 2).ToArray();
        }
    }




    public NeuralNetwork(int[] layerSizes)
        // yes this is very beautiful
        : this(layerSizes[0], layerSizes.Skip(1).Take(layerSizes.Length-2).ToArray(), layerSizes[layerSizes.Length-1]){
  
    }

    public NeuralNetwork(int inputSize, int[] hiddenLayers, int outputSize) {

        layerSizes = new int[2 + hiddenLayers.Length];
        layerSizes[0] = inputSize;
        layerSizes[layerSizes.Length - 1] = outputSize;
        Array.Copy(hiddenLayers, 0, layerSizes, 1, hiddenLayers.Length);


        numberOfLayers = layerSizes.Length;

        activations = new Vector<float>[layerSizes.Length];
        weightedInputs = new Vector<float>[layerSizes.Length];
        biases = new Vector<float>[layerSizes.Length];
        for (int i = 0; i < layerSizes.Length; i++) {
            activations[i] = Vector<float>.Build.Dense(layerSizes[i]);
            weightedInputs[i] = Vector<float>.Build.Dense(layerSizes[i]);
            biases[i] = Vector<float>.Build.Dense(layerSizes[i]);
        }

        weights = new Matrix<float>[layerSizes.Length - 1];
        for (int i = 0; i < weights.Length; i++)
            weights[i] = Matrix<float>.Build.Dense(layerSizes[i + 1], layerSizes[i]);

        RandomizeWeightsAndBiases();

    }
        
    

    public void RandomizeWeightsAndBiases() {

        foreach (var w in weights)
            for (int r = 0; r < w.RowCount; r++)
                for (int c = 0; c < w.ColumnCount; c++)
                    if (useGuassian)
                        w[r, c] = Gaussian(0, .05f);
                    else
                        w[r, c] = UnityEngine.Random.Range(-1f, 1f);

        foreach (var v in biases)
            for (int i = 0; i < v.Count; i++)
                if(useGuassian)
                    v[i] = Gaussian(0, .05f);
                else
                    v[i] = UnityEngine.Random.Range(-1f, 1f);
    }

    public float[] FeedForward(IEnumerable<float> inputs) {
        
        activations[0].SetValues(inputs.ToArray());

        

        for (int i = 1; i < activations.Length; i++) {
            weightedInputs[i] = weights[i - 1] * activations[i - 1];
            if(useBiases)
                weightedInputs[i] += biases[i];
            if(useSigmoid)
                activations[i] = Vectorize(Sigmoid, weightedInputs[i]);
            else
                activations[i] = weightedInputs[i].PointwiseTanh();
        }
        return activations[numberOfLayers - 1].ToArray();
    }



    // move to genetic
    // in genetic, extract fitness
    // remove sort by nn
    public float fitness;
    public void Mutate(float probability, float mutationFactor)
    {
        for (int w = 0; w < weights.Length; w++)
        {
            for (int r = 0; r < weights[w].RowCount; r++)
            {
                for (int c = 0; c < weights[w].ColumnCount; c++)
                {
                    if (UnityEngine.Random.Range(0f, 1f) < probability)
                    {
                        float mutation = weights[w][r, c] * UnityEngine.Random.Range(-mutationFactor, mutationFactor);
                        weights[w][r, c] += mutation;
                    }
                }
            }
        }
    }


    public int CompareTo(object obj) {
        return fitness.CompareTo(((NeuralNetwork)obj).fitness);
    }


    public NeuralNetwork Clone() {

        NeuralNetwork nn = new NeuralNetwork(layerSizes);
        for (int i = 0; i < weights.Length; i++)
            weights[i].CopyTo(nn.weights[i]);
        for (int i = 0; i < biases.Length; i++)
            biases[i].CopyTo(nn.biases[i]);
        return nn;
    }



    static Vector<float> Vectorize(Func<float, float> function, IEnumerable<float> a) {
        return Vector<float>.Build.DenseOfArray(a.Select(x => function(x)).ToArray());
    }

    static float Sigmoid(float x) {
        return 1f / (1f + (float)Math.Exp(-x));
    }
    
    static float Gaussian(float mean, float standardDeviation) {
        // https://stackoverflow.com/questions/218060/random-gaussian-variables

        float uniform1 = 1.0f - UnityEngine.Random.value;
        double uniform2 = 1.0f - UnityEngine.Random.value;
        double randomStandardNormal = Math.Sqrt(-2.0 * Math.Log(uniform1)) * Math.Sin(2.0 * Math.PI * uniform2);
        float randomNormal = mean + standardDeviation * (float)randomStandardNormal;
        return randomNormal;
    }
}