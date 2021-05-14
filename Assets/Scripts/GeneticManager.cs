using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GeneticManager : MonoBehaviour
{



    #region parameters

    public bool loadFileOnStartup = false;

    public int inputSize = 11;
    public int[] hiddenLayerSizes;

    [Range(10, 100)]
    public int populationSize = 15;
    public float populationWinnerPercentage = 0.3f;


    [Range(0.01f, 0.99f)]
    public float mutationProbability = 0.4f;
    [Range(0.01f, 0.5f)]
    public float mutationFactor = 0.1f;


    public bool randomNetworks = true;
    public int newRandomsCount = 3;

    #endregion


    #region test termination criteria

    public bool distanceTravelledLimitEnabled = false;
    public int distanceTravelledLimit = 1400;
    public float longestDistanceTravelled = 0f;

    public bool fitnessLimitEnabled = false;
    public int fitnessLimit = 10000;

    public float TimeLimit;

    public bool neverEndTest = false;
    #endregion



    #region training analysis data
    public ulong
        endCauseTimeCount = 0,
        endCauseWallCount = 0,
        endCauseRockCount = 0;
    public float fitnessHighScore = 0;
    
    public float distanceHighScore = 0;

    public List<float> generationalFitnesses = new List<float>();

    #endregion




    public static GeneticManager instance;
    public bool autoSave = true;
    
    public float[] fitnesses;

    public NeuralNetwork[] population;
    public int currentNetwork = 0;
    public int currentGeneration = 0;


    public FitnessEvaluator fitnessEvaluator;
    public DistanceNNInterface distanceNNInterface;

    
    
    


    private void UpdatePublicFittnessesArray()
    {
        for (int i = 0; i < population.Length; i++)      
            fitnesses[i] = population[i].fitness;      
    }
 

    private void Start()
    {
        instance = this;
        distanceNNInterface = fitnessEvaluator.GetComponent<DistanceNNInterface>();
    }

    public void StartTests()
    {
        GeneratePopulation();
        fitnesses = new float[populationSize];
        StartNewTest();
    }


    
    private void Update()
    {
        UpdatePublicFittnessesArray();
    }




    void GeneratePopulation()
    {
        if (loadFileOnStartup)     
            FileHandler.LoadNNfromXML(this);      
        else
        {
            population = new NeuralNetwork[populationSize];
            for (int i = 0; i < populationSize; i++)
                population[i] = new NeuralNetwork(inputSize, hiddenLayerSizes, 2);
        }
    }

    
    public void EndGeneration()
    {
        // sort networks in descending order by fitness
        System.Array.Sort(population);
        System.Array.Reverse(population);

        if((distanceTravelledLimitEnabled && longestDistanceTravelled > distanceTravelledLimit)
            || (fitnessLimitEnabled && population[0].fitness > fitnessLimit) )
        {
            
            Application.Quit();
        }
        if(autoSave)
            FileHandler.SaveNNToXML(this);


        // select the best networks
        int numberOfWinners = Mathf.RoundToInt(populationWinnerPercentage * populationSize);

        generationalFitnesses.Add(population[0].fitness);

        // overwrite the loser networks
        for (int i = numberOfWinners - 1; i < populationSize; i++)
            population[i] = population[i % numberOfWinners].Clone();

        // then mutate them
        for (int i = numberOfWinners - 1; i < populationSize; i++)
            population[i].Mutate(mutationProbability, mutationFactor);

        // add completely new random ones at the end, just in case entire generation was retarded
        if (randomNetworks)
        {
            //int newRandomsCount = 3;
            for (int i = populationSize - newRandomsCount; i < populationSize; i++)
                population[i] = new NeuralNetwork(inputSize, hiddenLayerSizes, 2);
        }

        currentNetwork = 0;
        currentGeneration++;
        longestDistanceTravelled = 0;

        StartNewTest();
    }



    public void EndTest()
    {
        if (neverEndTest)
            return;

        
        population[currentNetwork].fitness = fitnessEvaluator.fitness;
        switch (fitnessEvaluator.testTerminationCause) {
            case FitnessEvaluator.TestTerminationCause.retardation:
            case FitnessEvaluator.TestTerminationCause.time:
            case FitnessEvaluator.TestTerminationCause.user:
                endCauseTimeCount++;
                break;
            case FitnessEvaluator.TestTerminationCause.walls:
                endCauseWallCount++;
                break;
            case FitnessEvaluator.TestTerminationCause.rock:
                endCauseRockCount++;
                break;
        }


        if (fitnessEvaluator.fitness > fitnessHighScore)
            fitnessHighScore = fitnessEvaluator.fitness;
        if (fitnessEvaluator.distanceTravelled > fitnessHighScore)
            distanceHighScore = fitnessEvaluator.distanceTravelled;

        if (fitnessEvaluator.distanceTravelled > longestDistanceTravelled)
            longestDistanceTravelled = fitnessEvaluator.distanceTravelled;


        currentNetwork++;

        if (currentNetwork < populationSize) 
            StartNewTest();
          else 
            EndGeneration();       
    }

    void StartNewTest()
    {
        distanceNNInterface.neuralNetwork = population[currentNetwork];
        fitnessEvaluator.StartTest();
    }
}
