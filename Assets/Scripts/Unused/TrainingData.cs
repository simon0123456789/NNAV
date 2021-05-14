using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class TrainingData {
    public enum Side : int
    {
        left = 0,
        right = 1
    }
    public float[] inputs; // lutning 
    public float[] outputs; //styrning 
    public TrainingData()
    {

    }

    public TrainingData(int inputsize, int outputsize)
    {
        inputs  = new float[inputsize];
        outputs = new float[outputsize];
    }




    public static void SaveToFile(List<TrainingData> trainingData, string filename) {
        Stream file = File.OpenWrite(filename);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, trainingData);
        file.Close();
    }

    public static List<TrainingData> LoadFromFile(string filename) {

        Stream file = File.OpenRead(filename);
        BinaryFormatter bf = new BinaryFormatter();
        var trainingData = bf.Deserialize(file) as List<TrainingData>;
        file.Close();
        return trainingData;

    }

    public static List<List<TrainingData>> CategorizeByTurn(List<TrainingData> data) {

        List<TrainingData> superlefts = new List<TrainingData>();
        List<TrainingData> lefts = new List<TrainingData>();
        List<TrainingData> straights = new List<TrainingData>();
        List<TrainingData> rights = new List<TrainingData>();
        List<TrainingData> superRights = new List<TrainingData>();

        foreach(var d in data) {

            float leftSteer = d.outputs[0];
            float rightSteer = d.outputs[1];



            float threshold = 0.05f;
            float threshold2 = .5f;
            float tilt = rightSteer - leftSteer;
            if (tilt < -threshold2) superlefts.Add(d);
            else if (tilt < -threshold) lefts.Add(d);
            else if (tilt < threshold) straights.Add(d);
            else if (tilt < threshold2) rights.Add(d);
            else superRights.Add(d);
            

        }

        List<List<TrainingData>> result = new List<List<TrainingData>>();
        result.Add(superlefts);
        result.Add(lefts);
        result.Add(straights);
        result.Add(rights);
        result.Add(superRights);

        return result;


    }




    public override string ToString() {
        string str = "inputs: ";
        foreach (var i in inputs) str += $"{i} ";
        str += "\noutputs: ";
        foreach (var o in outputs) str += $"{o} ";
        return str;
    }
}