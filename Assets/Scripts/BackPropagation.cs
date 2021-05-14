using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Linq;

public class BackPropagation : MonoBehaviour
{


    





    private void Start() {

        BooleanTraining();
    
    }











    void BooleanTraining() { 

        float[] val0 = { 0f };
        float[] val1 = { 1f };

        float[] val00 = { 0f, 0f };
        float[] val01 = { 0f, 1f };
        float[] val10 = { 1f, 0f };
        float[] val11 = { 1f, 1f };



        TrainingData[] unityTestData = new TrainingData[] {
            new TrainingData{ inputs = val0, outputs = val0 },
            new TrainingData{ inputs = val1, outputs = val1 }
        };

        TrainingData[] andTestData = new TrainingData[] {
            new TrainingData{inputs = val00,outputs = val0},
            new TrainingData{inputs = val01,outputs = val0},
            new TrainingData{inputs = val10, outputs = val0},
            new TrainingData{inputs = val11, outputs = val1}
        };

        TrainingData[] orTestdata = new TrainingData[] {
            new TrainingData{inputs = val00,outputs = val0},
            new TrainingData{inputs = val01,outputs = val1},
            new TrainingData{inputs = val10, outputs = val1},
            new TrainingData{inputs = val11, outputs = val1}
        };

        TrainingData[] xorTestData = new TrainingData[] {
            new TrainingData{inputs = val00,outputs = val0},
            new TrainingData{inputs = val01,outputs = val1},
            new TrainingData{inputs = val10, outputs = val1},
            new TrainingData{inputs = val11, outputs = val0}

        };


        int[] unityNetTopology = { 1, 2, 1 };
        int[] binaryTopology = { 2, 4, 4, 1 };


        NeuralNetwork unityNet = new NeuralNetwork(unityNetTopology) { useBiases = true, useGuassian = true, useSigmoid = true };
        NeuralNetwork andNet    = new NeuralNetwork(binaryTopology) { useBiases = true, useGuassian = true, useSigmoid = true };
        NeuralNetwork orNet     = new NeuralNetwork(binaryTopology) { useBiases = true, useGuassian = true, useSigmoid = true };
        NeuralNetwork xorNet    = new NeuralNetwork(binaryTopology) { useBiases = true, useGuassian = true, useSigmoid = true };




        TrainingData[] testData = andTestData;
        NeuralNetwork net = andNet;



        Debug.Log(printTest(testData, net));
        for (int i = 0; i < 100; i++)
            StochasticGradientDescent(net, testData, 1000, 1, 10f);
        Debug.Log(printTest(testData, net));
    }

    string printTest(TrainingData[] data, NeuralNetwork nn) {
        string str = "";
        foreach (var d in data) {
            str += "\ninput: ";
            foreach (var i in d.inputs) str += $"{i} ";
            str += "\nexpected: ";
            foreach (var e in d.outputs) str += $"{e} ";
            str += "\nactual: ";
            foreach (var a in nn.FeedForward(d.inputs)) str += $"{a} ";
            str += $"\ncost: {CalculateCost(nn, d)}";

        }
        str += $"\naverage cost: {CalculateAverageCost(nn, data)}\n";
        return str;
    }



    public float CalculateCost(NeuralNetwork nn, TrainingData data) {
        return Vectorize(Mse, nn.FeedForward(data.inputs), data.outputs).Sum();
    }

    public float CalculateAverageCost(NeuralNetwork nn, TrainingData[] data) {
        return data.Select(x => CalculateCost(nn, x)).Sum() / data.Length;
    }



    // shuffles trainingData into batches, and does gradient descent over each batch, a given number of epochs
    public static void StochasticGradientDescent(NeuralNetwork nn, TrainingData[] trainingData, int epochs, int batchSize, float learningRate) {



        for (int i = 0; i < epochs; i++) {

            // shuffle training data
            System.Random random = new System.Random();
            TrainingData[] shuffled = trainingData.OrderBy(item => random.Next()).ToArray();

            // divide into batches
            List<List<TrainingData>> batches = new List<List<TrainingData>>();
            for (int j = 0; j < trainingData.Length; j += batchSize) {
                batches.Add(trainingData.ToList().GetRange(j, Math.Min(batchSize, trainingData.Length - j)));
            }


            float averageCost = 0f;


            for (int j = 0; j < batches.Count; j++) {
                averageCost += DoGradientDescent(nn, batches[j].ToArray(), learningRate);
            }

            averageCost /= trainingData.Length;

            string analysis = $"epoch {i}, avg err {averageCost}";
            Debug.Log(analysis);

        }


    }

    // calculates the average delta from a batch, and applies it with learningRate
    static float DoGradientDescent(NeuralNetwork nn, TrainingData[] batch, float learningRate) {

        // initialise nablaBiases and nablaWeights to be zero'ed vectors of the same dimensions as in network
        NablaVector nabla = new NablaVector(nn.biases, nn.weights);
        nabla.MakeZero();
        float cost = 0f;

        // sum the nablas for each trainingData
        foreach (TrainingData trainingData in batch) {
            nabla += Backpropagate(nn, trainingData);
            float deltaCost = Vectorize(Mse, nn.activations[nn.numberOfLayers - 1], trainingData.outputs).Sum();
            cost += deltaCost;
        }


        // apply nablas
        for (int i = 0; i < nn.weights.Length; i++)
            nn.weights[i] -= learningRate / batch.Length * nabla.weights[i];

        for (int i = 0; i < nn.biases.Length; i++)
            nn.biases[i] -= learningRate / batch.Length * nabla.biases[i];


        return cost / batch.Length;

    }

    // returns vector of deltas to weights and biases, that will lead to decreased cost, defined by trainingData
    static NablaVector Backpropagate(NeuralNetwork nn, TrainingData trainingData) {

        // intitialize nabla biases and weights to be zero'ed and of same dimensions as in network
        NablaVector nabla = new NablaVector(nn.biases, nn.weights);
        nabla.MakeZero();

        // feedforward training data inputs
        nn.FeedForward(trainingData.inputs);


        // backward pass
        // since delta will always be the same as biases, we don't create a separate vector
        int L = nn.numberOfLayers - 1;     // index of last layer       

        // error for output layer is: C'(a^L) hadamard with a^L'(z^L)
        nabla.biases[L] = Vectorize(MsePrime, nn.activations[nn.numberOfLayers - 1], trainingData.outputs).PointwiseMultiply(Vectorize(SigmoidPrime, nn.weightedInputs[L]));
        // must convert to mx1 and 1xn matrices for correct operation
        nabla.weights[L - 1] = nabla.biases[L].ToColumnMatrix() * nn.activations[L - 1].ToRowMatrix();



        // calculate error in remaining layers
        for (int l = L - 1; l > 0; l--) {

            var a = nn.weights[l].Transpose() * nabla.biases[l + 1].ToColumnMatrix();
            var b = Vectorize(SigmoidPrime, nn.weightedInputs[l]);

            // conversation necessary to avoid lots of casts
            var c = Vector<float>.Build.DenseOfArray(a.ToColumnArrays()[0]);

            nabla.biases[l] = c.PointwiseMultiply(b);
            nabla.weights[l - 1] = nabla.biases[l].ToColumnMatrix() * nn.activations[l - 1].ToRowMatrix();
        }

        string str =
                $"training data:\n {trainingData}\n " +
                $"actual: {(nn.activations[L].ToVectorString().Replace("\n", " "))}\n" +
                $"current config: \n{new NablaVector(nn.biases, nn.weights)}\n" +
                $"proposed delta:\n {nabla}";



        return nabla;

    }





    static Vector<float> Vectorize(Func<float, float> function, IEnumerable<float> a) {
        return Vector<float>.Build.DenseOfArray(a.Select(x => function(x)).ToArray());
    }

    static Vector<float> Vectorize(Func<float, float, float> function, IEnumerable<float> a, IEnumerable<float> b) {
        return Vector<float>.Build.DenseOfArray(a.Zip(b, (x, y) => function(x, y)).ToArray());
    }




    static float Sigmoid(float x) {
        return 1f / (1f + (float)Math.Exp(-x));
    }

    static float SigmoidPrime(float x) {
        return Sigmoid(x) * (1f - Sigmoid(x));
    }



    static float Mse(float activation, float y) {
        return (activation - y) * (activation - y);
    }

    static float MsePrime(float activation, float y) {
        return activation - y;
    }


    // class for storing deltas for biases and weights. 
    public class NablaVector {

        public Vector<float>[] biases;
        public Matrix<float>[] weights;

        // creates a copy of the biases and weights
        public NablaVector(Vector<float>[] biases, Matrix<float>[] weights) {
            this.biases = new Vector<float>[biases.Length];
            for (int i = 0; i < biases.Length; i++)
                this.biases[i] = Vector<float>.Build.DenseOfVector(biases[i]);

            this.weights = new Matrix<float>[weights.Length];
            for (int i = 0; i < weights.Length; i++)
                this.weights[i] = Matrix<float>.Build.DenseOfMatrix(weights[i]);
        }

        // sets all fields to zero
        public void MakeZero() {
            foreach (var b in biases) b.Clear();
            foreach (var w in weights) w.Clear();
        }

        // sums two NablaVectors of same dimensions
        public static NablaVector operator +(NablaVector a, NablaVector b) {
            NablaVector sum = new NablaVector(a.biases, a.weights);
            for (int i = 0; i < a.biases.Length; i++)
                sum.biases[i] = a.biases[i] + b.biases[i];
            for (int i = 0; i < a.weights.Length; i++)
                sum.weights[i] = a.weights[i] + b.weights[i];
            return sum;
        }

        public override string ToString() {
            string str = "biases:\n";
            for (int i = 0; i < biases.Length; i++) str += $"{i}: [{(biases[i].ToVectorString().Replace("\n", " "))}]\n";
            str += "weights:\n";
            for (int i = 0; i < weights.Length; i++) str += $"{i}:\n {weights[i].ToMatrixString()}";
            return str;
        }

    }

}
