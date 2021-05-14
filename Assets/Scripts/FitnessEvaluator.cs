using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FitnessEvaluator : MonoBehaviour
{


    public enum TestTerminationCause { time, user, walls, rock, retardation}
    [HideInInspector]
    public TestTerminationCause testTerminationCause = TestTerminationCause.user;

    public bool forceEndTest = false;
    public float fitness = 0;

    public float timeLimit = 0f;            // timeLimit == 0 means no time limit
    public float defaultTimeLimit = 20f;
    public float elapsedTime = 0;
    float startTime = 0;
    

    public bool stoneCollisionEndsTest = true;
    public bool collisionEndsTest = false;

    public float distanceExtraTime = 20f;
    public float distanceTravelled = 0f;
    float distanceAccumulator = 0f;         // for rewarding extra time if they cover enough distance

    [HideInInspector]
    public Vector3 spawnPosition;
    [HideInInspector]
    public Quaternion spawnRotation;


    [HideInInspector]
    public Transform currentSpawn;

    Vector3 previousPosition;
    Transform trans;
    Rigidbody rb;

    private void Start()
    {
        trans = GetComponent<Transform>();
        spawnPosition = trans.position;
        spawnRotation = trans.rotation;
        rb = GetComponent<Rigidbody>();
    }

    
    public void StartTest()
    {
        trans.SetPositionAndRotation(spawnPosition, spawnRotation);
        rb.velocity = new Vector3(0, 0, 0);
        rb.Sleep();
        
        startTime = Time.time;
        previousPosition = trans.position;
        distanceTravelled = 0;
        distanceAccumulator = 0;
        timeLimit = defaultTimeLimit;

    }

    
    private void Update()
    {
        elapsedTime = Time.time - startTime;

        // deltaDistance is just difference between our current and last position, and the dot product determines if we're moving backwards or forwards
        float deltaDistance = Vector3.Distance(trans.position, previousPosition) * Mathf.Sign(Vector3.Dot(rb.velocity, trans.forward));
        previousPosition = trans.position;
        distanceTravelled += deltaDistance;

        // reward some extra time if they cover enough distance
        distanceAccumulator += deltaDistance;
        if (distanceAccumulator > 100f) {
            distanceAccumulator -= 100f;
            timeLimit += 20f;
        }

        // calculate current fitness
        float timeFactor = 0.2f;
        fitness = distanceTravelled - elapsedTime * timeFactor;

        if (forceEndTest) {
            testTerminationCause = TestTerminationCause.user;
            EndTest();
        }

        // this net is probably too stupid, lets end it early
        if (fitness < -2f) {
            testTerminationCause = TestTerminationCause.retardation;
            EndTest();
        }
            
        // ran out of time
        if(timeLimit > 0 && elapsedTime > timeLimit)
        {
            testTerminationCause = TestTerminationCause.time;
            EndTest();
        }

        if (trans.position.y < -15)
        {
            fitness -= 20f;
            testTerminationCause = TestTerminationCause.walls;
            EndTest();
        }
           
       
    }


    void EndTest()
    {
        GeneticManager.instance.EndTest();
    }


    private void OnCollisionEnter(Collision collision)
    {
        

        if (collision.gameObject.name.Contains("Walls") || collision.gameObject.name.Contains("Barrier"))
        {
            testTerminationCause = TestTerminationCause.walls;
            EndTest();
        }
        
        else if (stoneCollisionEndsTest && collision.gameObject.name.Contains("Rock"))
        {
            testTerminationCause = TestTerminationCause.rock;
            EndTest();
        }

    }



}
