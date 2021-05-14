using UnityEngine;
using System.IO;
using System.Xml;
using MathNet.Numerics.LinearAlgebra;
using System;



public static class FileHandler
{
    const string folderName = "NetworkGenerations";
    static public float timeFromFileIRL = 0;
    static public float timeFromFileSim = 0;
    static public string fileComment = "";
    static public string loadFile = "network.xml";
    static public string saveFile;
    static public string tupel;

    private static XmlAttribute SimpleAttribute(XmlDocument doc, string name, string value)
    {
        XmlAttribute attribute = doc.CreateAttribute(name);
        attribute.Value = value;
        return attribute;
    }
    private static XmlElement GetXMLElement(XmlElement xmlElement, string name)
    {
        foreach (XmlElement child in xmlElement.ChildNodes)
            if (child.Name == name)
                return child;

        return null;
    }
    private static XmlElement GetXMLElement(XmlNode xmlElement, string name)
    {
        foreach (XmlNode child in xmlElement.ChildNodes)
            if (child.Name == name)
                return (XmlElement)child;

        return null;
    }

    // old 
    private static XmlElement NetworkLayerInfoToXmlElement(XmlDocument doc, NeuralNetwork network)
    {
        XmlElement layerInfo = doc.CreateElement("LayerInfo");

        XmlElement inputLayerSize = doc.CreateElement("inputLayerSize");
        inputLayerSize.InnerText = network.inputLayer.Count.ToString();
        layerInfo.AppendChild(inputLayerSize);

        XmlElement hiddenLayerSizes = doc.CreateElement("HiddenLayerSizes");
        foreach (var hiddenLayer in network.hiddenLayers)
        {
            XmlElement size = doc.CreateElement("Size");   
            size.InnerText = hiddenLayer.Count.ToString(); 
            hiddenLayerSizes.AppendChild(size);
        }
        layerInfo.AppendChild(hiddenLayerSizes);


        XmlElement outputLayerSize = doc.CreateElement("outputLayerSize");
        outputLayerSize.InnerText = network.outputLayer.Count.ToString();
        layerInfo.AppendChild(outputLayerSize);

        return layerInfo;
    }
    private static XmlElement NNLayerstoXmlElement(XmlDocument doc, NeuralNetwork network)
    {
        XmlElement networkElement = doc.CreateElement("NetworkLayers");

        networkElement.Attributes.Append(SimpleAttribute(doc, "Fitness", network.fitness.ToString()));

        for (int currentLayer = 0; currentLayer < network.weights.Length; currentLayer++)
        {
            XmlElement layer = doc.CreateElement("Layer");
            for (int currentRow = 0; currentRow < network.weights[currentLayer].RowCount; currentRow++)
            {
                XmlElement row = doc.CreateElement("Row");
                for (int currentColumn = 0; currentColumn < network.weights[currentLayer].ColumnCount; currentColumn++)
                {
                    XmlElement column = doc.CreateElement("Column");
                    column.InnerText = network.weights[currentLayer][currentRow, currentColumn].ToString();
                    row.AppendChild(column);
                }
                layer.AppendChild(row);
            }
            networkElement.AppendChild(layer);
        }
        return networkElement;
    }
    private static XmlElement NeuralNetworkElement(XmlDocument doc, NeuralNetwork network)
    {
        XmlElement neuralNetworkElement = doc.CreateElement("NeuralNetwork");

        neuralNetworkElement.AppendChild(NetworkLayerInfoToXmlElement(doc, network));
        neuralNetworkElement.AppendChild(NNLayerstoXmlElement(doc, network));

        return neuralNetworkElement;
    }


    public static XmlElement MatrixListToXmlElement(XmlDocument doc, Matrix<float>[] matrixList, string name = "") // New
    {
        if (name.Length < 1)
            name = "MatrixList";
        XmlElement xmlMatrixList = doc.CreateElement(name);
        xmlMatrixList.Attributes.Append(SimpleAttribute(doc, "Type", "MatrixList"));
        foreach (Matrix<float> matrix in matrixList)
        {
            XmlElement xmlMatrix = MatrixToXmlElement(doc, matrix);
            xmlMatrixList.AppendChild(xmlMatrix);
        }
        xmlMatrixList.Attributes.Append(SimpleAttribute(doc, "Lenght", matrixList.Length.ToString()));

        return xmlMatrixList;

    }
    public static XmlElement MatrixToXmlElement(XmlDocument doc, Matrix<float> matrix) // New
    {
        int rows = matrix.RowCount;
        int colums = matrix.ColumnCount;
        XmlElement xmlMatrix = doc.CreateElement("Matrix");
        for (int r = 0; r <rows; r++)
        {
            XmlElement row = doc.CreateElement("Row");
            
            for (int c = 0; c < colums; c++)
            {
                XmlElement column = doc.CreateElement("Column");
                column.InnerText = matrix[r, c].ToString();
                row.AppendChild(column);
            }
            xmlMatrix.AppendChild(row);
        }
        xmlMatrix.Attributes.Append(SimpleAttribute(doc, "Rows", rows.ToString()));
        xmlMatrix.Attributes.Append(SimpleAttribute(doc, "Colums", colums.ToString()));
        return xmlMatrix;
    }
    public static Matrix<float> XmlElementToMatrix(XmlElement xmlMatrix) // New
    {
        if(xmlMatrix.HasAttribute("Rows") && xmlMatrix.HasAttribute("Colums"))
        {
            if( int.TryParse(xmlMatrix.GetAttribute("Rows"), out int rows) &&
                int.TryParse(xmlMatrix.GetAttribute("Colums"), out int colums))
            {
                Matrix<float> matrix = Matrix<float>.Build.Dense(rows, colums);

                for(int row = 0; row< xmlMatrix.ChildNodes.Count; row++)
                {
                    for(int column = 0; column < xmlMatrix.ChildNodes[row].ChildNodes.Count; column++)
                    {
                        if(float.TryParse(xmlMatrix.ChildNodes[row].ChildNodes[column].InnerText, out float value))
                        {
                            matrix[row, column] = value;
                        }
                    }
                }
                return matrix;
            }
        }
        return null;
    }
    public static Matrix<float>[] XmlElementToMatrixList(XmlElement xmlMatrixList) // New
    {
       if( xmlMatrixList.HasAttribute("Lenght"))
        {
            if(int.TryParse(xmlMatrixList.GetAttribute("Lenght"), out int lenght))
            {
                Matrix<float>[] matrixList = new Matrix<float>[lenght];
                for(int i = 0; i<lenght; i++)
                {
                    Matrix<float> matrix = XmlElementToMatrix((XmlElement)xmlMatrixList.ChildNodes[i]);
                    if (matrix != null)
                        matrixList[i] = matrix;
                }
                return matrixList;
            }
        }
        return null;
    }



    static void TupelToLayers(string tupel, out int inputSize, out int[] hiddenSizes, out int outputSize)
    {
        string[] split = tupel.Split('-');

        hiddenSizes = new int[split.Length - 2];
        for(int i = 1; i<split.Length-1; i++)
            hiddenSizes[i-1] = int.Parse(split[i]);

        inputSize = int.Parse(split[0]);
        outputSize = int.Parse(split[split.Length - 1]);
    }

    public static void LoadNNfromXML(GeneticManager geneticManager)
    {
        if (!Directory.Exists(folderName) || !File.Exists(folderName + "/" + loadFile))
        {
            Menu.ErrorMsg($"Failed to NN from File\nFolder '{folderName}' or file '{loadFile} does not exist");
            return;
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(loadFile);
        XmlElement root = (XmlElement)doc.FirstChild;

        if (!root.HasAttribute("XmlLayoutVersion"))
        {
           geneticManager.population = LoadNNfromXML(loadFile, 
               ref geneticManager.currentGeneration,
               ref timeFromFileIRL,
               ref timeFromFileSim,
               ref geneticManager.inputSize,
               ref geneticManager.endCauseTimeCount, 
               ref geneticManager.endCauseWallCount, 
               ref geneticManager.endCauseRockCount);
            return;
        }

        int.TryParse(root.GetAttribute("XmlLayoutVersion"), out int xmlLayoutVersion);
        if (xmlLayoutVersion != 3)
            return;

        XmlElement additionalSettingsAndData = GetXMLElement(root, "AdditionalSettingsAndData");

        FileHandler.tupel = GetXMLElement(additionalSettingsAndData, "Tupel").InnerText;
        TupelToLayers(FileHandler.tupel, out int inputSize, out int[] hiddenSizes, out int outputSize);

        XmlElement generation = GetXMLElement(root, "Generation");
        geneticManager.population = new NeuralNetwork[generation.ChildNodes.Count];
        for(int i = 0; i< generation.ChildNodes.Count; i++)
        {
            XmlElement weightMatrix = GetXMLElement(generation.ChildNodes[i], "Weights");
            geneticManager.population[i] = new NeuralNetwork(inputSize, hiddenSizes, outputSize);
            geneticManager.population[i].weights = XmlElementToMatrixList(weightMatrix);
        }

        XmlElement timeData = GetXMLElement(root, "TimeData");
        if(timeData != null)
        {
            float.TryParse(timeData.GetAttribute("TimeElapsedSimulated"), out timeFromFileSim);
            float.TryParse(timeData.GetAttribute("TimeElapsedIRL"), out timeFromFileIRL);
        }

        XmlElement highScores = GetXMLElement(root, "HighScores");
        if(highScores != null)
        {
            float.TryParse(highScores.GetAttribute("DistanceHighScore"), out geneticManager.distanceHighScore);
            float.TryParse(highScores.GetAttribute("FitnessHighScore"), out geneticManager.fitnessHighScore);
        }

        XmlElement endsBy = GetXMLElement(root, "endsBy");
        if(endsBy != null)
        {
            ulong.TryParse(endsBy.GetAttribute("EndsByTime"), out geneticManager.endCauseTimeCount);
            ulong.TryParse(endsBy.GetAttribute("EndsByRock"), out geneticManager.endCauseRockCount);
            ulong.TryParse(endsBy.GetAttribute("EndsByWall"), out geneticManager.endCauseWallCount);
        }

        XmlElement populationInfo = GetXMLElement(root, "PopulationInfo");
        if(populationInfo != null)
        {
            //float.TryParse(populationInfo.GetAttribute("PopulationSize"), out geneticManager.populationSize);
            float.TryParse(populationInfo.GetAttribute("populationWinnerPercentage"), out geneticManager.populationWinnerPercentage);
        }

        XmlElement mutationInfo = GetXMLElement(root, "Mutation");
        if(mutationInfo != null)
        {
            float.TryParse(mutationInfo.GetAttribute("MutationProbability"), out geneticManager.mutationProbability);
            float.TryParse(mutationInfo.GetAttribute("MutationFactor"), out geneticManager.mutationProbability);
        }

        XmlElement distanceGoal = GetXMLElement(root, "DistanceGoal");
        if(distanceGoal != null)
        {
            int.TryParse(distanceGoal.InnerText, out geneticManager.distanceTravelledLimit);
            bool.TryParse(distanceGoal.GetAttribute("Enabled"), out geneticManager.distanceTravelledLimitEnabled);
        }

        XmlElement fitnessGoal = GetXMLElement(root, "FitnessGoal");
        if (fitnessGoal != null)
        {
            int.TryParse(fitnessGoal.InnerText, out geneticManager.fitnessLimit);
            bool.TryParse(fitnessGoal.GetAttribute("Enabled"), out geneticManager.fitnessLimitEnabled);
        }

        XmlElement genInfo = GetXMLElement(root,"GenerationInfo");
        if(genInfo != null)
        {
            int.TryParse(genInfo.GetAttribute("CurrentGeneration"), out geneticManager.currentGeneration);
            //XmlElement fitnesses = GetXMLElement(genInfo, "Fitnesses");
        }

    }
 

    public static void SaveNNToXML(GeneticManager geneticManager, params string[] extraInfo)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("NNAV");
        root.Attributes.Append(SimpleAttribute(doc, "XmlLayoutVersion", "3"));

        XmlElement settingsAndData = doc.CreateElement("AdditionalSettingsAndData");
        XmlElement tupelen = doc.CreateElement("Tupel");
        tupelen.InnerText = tupel;
        settingsAndData.AppendChild(tupelen);

        XmlElement timeData = doc.CreateElement("TimeData");
        timeData.Attributes.Append(SimpleAttribute(doc, "TimeElapsedSimulated",     $"{Time.time + timeFromFileSim}" ));
        timeData.Attributes.Append(SimpleAttribute(doc, "TimeElapsedIRL",           $"{Time.realtimeSinceStartup + timeFromFileIRL}"));
        timeData.Attributes.Append(SimpleAttribute(doc, "ElapsedSimulatedSeconds",  $"{System.TimeSpan.FromSeconds((uint)(Time.time + timeFromFileSim))}"));
        timeData.Attributes.Append(SimpleAttribute(doc, "ElapsedIRLSeconds",        $"{System.TimeSpan.FromSeconds((uint)(Time.realtimeSinceStartup + timeFromFileIRL))}"));
        settingsAndData.AppendChild(timeData);

        XmlElement highScores = doc.CreateElement("HighScores");
        highScores.Attributes.Append(SimpleAttribute(doc, "DistanceHighScore",  $"{geneticManager.distanceHighScore}"));
        highScores.Attributes.Append(SimpleAttribute(doc, "HighestFitness",     $"{geneticManager.population[0].fitness}"));
        highScores.Attributes.Append(SimpleAttribute(doc, "FitnessHighScore",   $"{geneticManager.fitnessHighScore}"));
        settingsAndData.AppendChild(highScores);

        XmlElement endsBy = doc.CreateElement("EndsBy");
        endsBy.Attributes.Append(SimpleAttribute(doc, "EndsByTime", $"{geneticManager.endCauseTimeCount}"));
        endsBy.Attributes.Append(SimpleAttribute(doc, "EndsByRock", $"{geneticManager.endCauseRockCount}"));
        endsBy.Attributes.Append(SimpleAttribute(doc, "EndsByWall", $"{geneticManager.endCauseWallCount}"));
        settingsAndData.AppendChild(endsBy);

        XmlElement populationInfo = doc.CreateElement("PopulationInfo");
        populationInfo.Attributes.Append(SimpleAttribute(doc, "PopulationSize",             $"{geneticManager.populationSize}"));
        populationInfo.Attributes.Append(SimpleAttribute(doc, "populationWinnerPercentage", $"{geneticManager.populationWinnerPercentage}"));
        settingsAndData.AppendChild(populationInfo);

        XmlElement mutationInfo = doc.CreateElement("Mutation");
        mutationInfo.Attributes.Append(SimpleAttribute(doc, "MutationProbability", $"{geneticManager.mutationProbability}"));
        mutationInfo.Attributes.Append(SimpleAttribute(doc, "MutationFactor",      $"{geneticManager.mutationFactor}"));
        settingsAndData.AppendChild(mutationInfo);

        XmlElement distanceGoal = doc.CreateElement("DistanceGoal");
        distanceGoal.Attributes.Append(SimpleAttribute(doc, "Enabled", $"{geneticManager.distanceTravelledLimitEnabled}"));
        distanceGoal.InnerText = $"{geneticManager.distanceTravelledLimit}";
        settingsAndData.AppendChild(distanceGoal);

        XmlElement fitnessGoal = doc.CreateElement("FitnessGoal");
        fitnessGoal.Attributes.Append(SimpleAttribute(doc, "Enabled", $"{geneticManager.fitnessLimit}"));
        fitnessGoal.InnerText = $"{geneticManager.fitnessLimit}";
        settingsAndData.AppendChild(fitnessGoal);

        XmlElement genInfo = doc.CreateElement("GenerationInfo");
        genInfo.Attributes.Append(SimpleAttribute(doc, "CurrentGeneration", $"{geneticManager.currentGeneration}"));
        XmlElement fitnesses = doc.CreateElement("GenerationFitnesses");
        for(int i = 0; i<geneticManager.fitnesses.Length; i++)
        {
            XmlElement fitness = doc.CreateElement("Fitness");
            fitness.Attributes.Append(SimpleAttribute(doc, "NetworkID", $"{i}"));
            fitness.InnerText = geneticManager.fitnesses[i].ToString();
            fitnesses.AppendChild(fitness);
        }
        genInfo.AppendChild(fitnesses);
        settingsAndData.AppendChild(genInfo);
        root.AppendChild(settingsAndData);

        XmlElement generation = doc.CreateElement("Generation");
        foreach(NeuralNetwork nn in geneticManager.population)
        {
            XmlElement neuralNetwork = doc.CreateElement("NeuralNetwork");
            neuralNetwork.Attributes.Append(SimpleAttribute(doc, "Fitness", nn.fitness.ToString()));
            neuralNetwork.AppendChild(NetworkLayerInfoToXmlElement(doc, nn));

            XmlElement matrixList = MatrixListToXmlElement(doc, nn.weights, "Weights");
            
            neuralNetwork.AppendChild(matrixList);
            generation.AppendChild(neuralNetwork);
        }
        root.AppendChild(generation);
        doc.AppendChild(root);

        if (!Directory.Exists(folderName))
            Directory.CreateDirectory(folderName);

        doc.Save(folderName + saveFile);

    }


    public static NeuralNetwork[] LoadNNfromXML(string filename, ref int currentGen, ref float timeFromFile, ref float timeFromFileSim, ref int rays, ref ulong endByTime, ref ulong endByWall, ref ulong endByRock)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(folderName + "/" + filename);

        XmlNode root = doc.FirstChild;

        if (root.Attributes.GetNamedItem("NewXmlLayout") != null)
        {
            if(bool.TryParse(root.Attributes.GetNamedItem("NewXmlLayout").Value, out bool NewXmlLayout))
            {
                XmlElement settingsAndData = GetXMLElement(root, "AdditionalSettingsAndData");

                //XmlElement nnSettings = GetXMLElement(settingsAndData, "NNSettings");
                //float populationWinnerPercentage = float.Parse(nnSettings.Attributes.GetNamedItem("populationWinnerPercentage").Value);
                //float mutationProbability = float.Parse(nnSettings.Attributes.GetNamedItem("mutationProbability").Value);
                //float mutationFactor = float.Parse(nnSettings.Attributes.GetNamedItem("mutationFactor").Value);
                //float populationSize = float.Parse(nnSettings.Attributes.GetNamedItem("populationSize").Value);

                //RaySettings - Not in use
                //TimeSettings - Not in use

                XmlElement timeData = GetXMLElement(settingsAndData, "TimeData");
                timeFromFileSim = float.Parse(timeData.Attributes.GetNamedItem("ElapsedSimulatedSeconds").Value);
                timeFromFile = float.Parse(timeData.Attributes.GetNamedItem("ElapsedIRLSeconds").Value);

                XmlElement generationInfo = GetXMLElement(settingsAndData, "GenerationInfo");
                currentGen = int.Parse(generationInfo.Attributes.GetNamedItem("CurrentGeneration").Value);

                XmlElement endsBy = GetXMLElement(settingsAndData, "EndsBy");
                endByTime = ulong.Parse(endsBy.Attributes.GetNamedItem("EndsByTime").Value);
                endByRock = ulong.Parse(endsBy.Attributes.GetNamedItem("EndsByRock").Value);
                endByWall = ulong.Parse(endsBy.Attributes.GetNamedItem("EndsByWall").Value);


            }

        }
        else
        {
            if (root.Name == "root")
                root = root.FirstChild.NextSibling; // Generation Attributet!

            if (root.Attributes.GetNamedItem("Time_ElapsedIRL_seconds") != null)
            {
                timeFromFile = float.Parse(root.Attributes.GetNamedItem("Time_ElapsedIRL_seconds").Value);
                Debug.Log("Time from file: " + timeFromFile);
            }
            else if (root.Attributes.GetNamedItem("Time_ElapsedIRL") != null)
            {
                timeFromFile = float.Parse(root.Attributes.GetNamedItem("Time_ElapsedIRL").Value);
                Debug.Log("Time from file: " + timeFromFile);
            }

            if (root.Attributes.GetNamedItem("Time_ElapsedSimulated_seconds") != null)
            {
                timeFromFileSim = float.Parse(root.Attributes.GetNamedItem("Time_ElapsedSimulated_seconds").Value);
                Debug.Log("Time from file: " + timeFromFile);
            }
            else if (root.Attributes.GetNamedItem("Time_ElapsedSimulated") != null)
            {
                timeFromFileSim = float.Parse(root.Attributes.GetNamedItem("Time_ElapsedSimulated").Value);
                Debug.Log("Time from file: " + timeFromFile);
            }


            Debug.Log("Curr >" + root.Attributes.GetNamedItem("CurrentGeneration").ToString());
            if (root.Attributes.GetNamedItem("CurrentGeneration") != null)
                currentGen = int.Parse(root.Attributes.GetNamedItem("CurrentGeneration").Value);
        }
        if (root.Name == "root")
            root = root.FirstChild.NextSibling;

        int populationSize = root.ChildNodes.Count;

        NeuralNetwork[] population = new NeuralNetwork[populationSize];
        int networkcounter = 0;
        foreach (XmlElement neuralNetwork in root.ChildNodes)
        {
            XmlNode networkLayerInfo = neuralNetwork.FirstChild;

            int inputLayerSize = int.Parse(networkLayerInfo.FirstChild.InnerText);
            rays = inputLayerSize;
            int hiddenLayersCount = networkLayerInfo.FirstChild.NextSibling.ChildNodes.Count;
            int[] hiddenLayerSizes = new int[hiddenLayersCount];
            for (int i = 0; i < hiddenLayersCount; i++)
            {
                hiddenLayerSizes[i] = int.Parse(networkLayerInfo.FirstChild.NextSibling.ChildNodes[i].InnerText);
            }
            int outputLayerSize = int.Parse(networkLayerInfo.FirstChild.NextSibling.NextSibling.InnerText);

            population[networkcounter] = new NeuralNetwork(inputLayerSize, hiddenLayerSizes, outputLayerSize);

            var network = neuralNetwork.FirstChild.NextSibling;
            population[networkcounter].fitness = float.Parse(network.Attributes.GetNamedItem("Fitness").Value);
            int layers = network.ChildNodes.Count;
            for (int currentLayer = 0; currentLayer < layers; currentLayer++)
            {
                int rows = network.ChildNodes[currentLayer].ChildNodes.Count;
                for (int currentRow = 0; currentRow < rows; currentRow++)
                {
                    int colums = network.ChildNodes[currentLayer].ChildNodes[currentRow].ChildNodes.Count;
                    for (int currentColumn = 0; currentColumn < colums; currentColumn++)
                    {
                        population[networkcounter].weights[currentLayer][currentRow, currentColumn] = float.Parse(network.ChildNodes[currentLayer].ChildNodes[currentRow].ChildNodes[currentColumn].InnerText);
                    }
                }
            }
            networkcounter++;
        }
        return population;
    }

    public static void LoadXMLSettings(GeneticManager geneticManager, FitnessEvaluator fitnessEvaluator, DistanceSensorArray distanceSensorArray) // Not Finished Yet
    {
        string settingsFilename = "settings.xml";

        if (!File.Exists(settingsFilename))
        {
            Menu.ErrorMsg($"Failed to load Xml Settings\n'{settingsFilename}'\nDoes not exist");
            return;
        }
           
        XmlDocument doc = new XmlDocument();
        doc.Load(settingsFilename);
        XmlNode root = doc.FirstChild;

        XmlElement general = GetXMLElement((XmlElement)root, "General");
        XmlElement xmlFileHandler = GetXMLElement((XmlElement)root, "FileHandler");
        XmlElement xmlGUI = GetXMLElement((XmlElement)root, "GUI");
        XmlElement input = GetXMLElement((XmlElement)root, "Input");
        XmlElement spawn = GetXMLElement((XmlElement)root, "Spawn");
        XmlElement rays = GetXMLElement((XmlElement)root, "Rays");
        XmlElement updateTimeScale = GetXMLElement((XmlElement)root, "UpdateTimeScale");
        XmlElement xmlFileComment = GetXMLElement((XmlElement)root, "FileComment");
        try
        {
            if (general != null)
            {

                tupel = GetXMLElement(general, "Tupel").InnerText;
                TupelToLayers(tupel, out Menu.numberOfRays, out int[] hiddenSizes, out int outputSize);
                for (int i = 0; i < hiddenSizes.Length - 1; i++)
                {
                    if (i > Menu.hiddenLayerSizes.Count - 1)
                        Menu.hiddenLayerSizes.Add(hiddenSizes[i]);
                    else
                        Menu.hiddenLayerSizes[i] = hiddenSizes[i];
                }
                Menu.hiddenLayers = hiddenSizes.Length;

                if (int.TryParse(GetXMLElement(general, "TimeScale").InnerText, out int _timeScale))
                    Time.timeScale = _timeScale;

                bool.TryParse(GetXMLElement(general, "AutofStart").InnerText, out Menu.autoStart);


                bool.TryParse(GetXMLElement(general, "DistanceGoal").GetAttribute("Enabled"), out geneticManager.distanceTravelledLimitEnabled);
                int.TryParse(GetXMLElement(general, "DistanceGoal").InnerText, out geneticManager.distanceTravelledLimit);

                int.TryParse(GetXMLElement(general, "FitnessGoal").InnerText, out geneticManager.fitnessLimit);
                bool.TryParse(GetXMLElement(general, "FitnessGoal").GetAttribute("Enabled"), out geneticManager.fitnessLimitEnabled);

                float.TryParse(GetXMLElement(general, "TimeLimits").GetAttribute("Default"), out fitnessEvaluator.defaultTimeLimit);
                float.TryParse(GetXMLElement(general, "TimeLimits").GetAttribute("Extra"), out fitnessEvaluator.defaultTimeLimit);

            }
            if (xmlFileHandler != null)
            {
                saveFile = GetXMLElement(xmlFileHandler, "SaveFile").InnerText;
                bool.TryParse(GetXMLElement(xmlFileHandler, "SaveFile").GetAttribute("AutoSave"), out geneticManager.autoSave);

                loadFile = GetXMLElement(xmlFileHandler, "LoadFile").InnerText;
                bool.TryParse(GetXMLElement(xmlFileHandler, "LoadFile").GetAttribute("LoadOnStart"), out geneticManager.loadFileOnStartup);

            }
            if (xmlGUI != null)
            {
                bool.TryParse(GetXMLElement(xmlGUI, "InfoPanel").InnerText, out Menu.drawInfoPanel);

                if (bool.TryParse(GetXMLElement(xmlGUI, "Camera").InnerText, out bool _camera))
                    if (GameObject.Find("Camera Rig") != null)
                        GameObject.Find("Camera Rig").SetActive(_camera);

                if (bool.TryParse(GetXMLElement(xmlGUI, "SpeedoMeter").InnerText, out bool _speedo))
                    if (GameObject.Find("Speedometer") != null)
                        GameObject.Find("Speedometer").SetActive(_speedo);

                if (bool.TryParse(GetXMLElement(xmlGUI, "nnDrawer").InnerText, out bool _nnDrawer))
                    if (GameObject.Find("NN Drawer") != null)
                        GameObject.Find("NN Drawer").SetActive(_nnDrawer);


                if (bool.TryParse(GetXMLElement(xmlGUI, "JoyStick").InnerText, out bool _joystick))
                    if (GameObject.Find("Joystick") != null)
                        GameObject.Find("Joystick").SetActive(_joystick);

            }
            if (input != null)
            {
                bool.TryParse(GetXMLElement(input, "DistanceRays").InnerText, out bool _distanceRays);
                bool.TryParse(GetXMLElement(input, "LineDetection").InnerText, out bool _linedetection);
                if (_distanceRays && _linedetection)
                    Menu.inputMode = DistanceNNInterface.InputMode.combo;
                else if (_distanceRays)
                    Menu.inputMode = DistanceNNInterface.InputMode.distance;
                else if (_linedetection)
                    Menu.inputMode = DistanceNNInterface.InputMode.line;

            }
            if (spawn != null)
            {

                if (bool.TryParse(GetXMLElement(spawn, "CrazyStones").InnerText, out bool _crazyStones) && _crazyStones)
                    new RockSpawner().GenerateCrazyStones();

                if (bool.TryParse(GetXMLElement(spawn, "DefaultRocks").InnerText, out bool _defaultRocks))
                    GameObject.Find("Rocks").SetActive(_defaultRocks);
                if (bool.TryParse(GetXMLElement(spawn, "Ramp").InnerText, out bool _ramp))
                    GameObject.Find("Ramp").SetActive(_ramp);
                if (bool.TryParse(GetXMLElement(spawn, "TestingRocks").InnerText, out bool _testingRocks))
                    GameObject.Find("TestingRocks").SetActive(_testingRocks);

            }
            if (rays != null)
            {

                //bool.TryParse(GetXMLElement(rays, "FixMaxDistanceBug").InnerText, out distanceSensorArray.fixMaxDistanceBug);
                float.TryParse(GetXMLElement(rays, "Distance").InnerText, out distanceSensorArray.maxDistance);
                float.TryParse(GetXMLElement(rays, "FOV").InnerText, out distanceSensorArray.fieldOfViewDeg);

            }
            if (updateTimeScale != null)
            {
                int.TryParse(updateTimeScale.GetAttribute("WhenDistance"), out Menu.updateTimeScaleWhenDistance);
                int.TryParse(updateTimeScale.GetAttribute("WhenFitness"), out Menu.updateTimeScaleWhenFitness);
                int.TryParse(updateTimeScale.GetAttribute("WhenIRLTime"), out Menu.updateTimeScaleWhenIRLTime);
                int.TryParse(updateTimeScale.GetAttribute("WhenSimTime"), out Menu.updateTimeScaleWhenSimTime);
                int.TryParse(updateTimeScale.InnerText, out Menu.updatedTimeScale);

            }
            if (xmlFileComment != null)
            {
                fileComment = xmlFileComment.InnerText;
            }
        }
        catch (Exception e) 
        { 
            Menu.ErrorMsg($"Failed to Load Xml Settings\n{e.Message}"); 
        }

        
    

    }

}