#define oldway

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

public class TrainingDataRecorder : MonoBehaviour
{
    List<TrainingData> trainingData = new List<TrainingData>();

    // Start is called before the first frame update

    DistanceNNInterface distanceNNInterface;
    CarInput carInput;

    VideoInput videoInput;
    public string filename = "data.txt";
    public bool recordData = false;
    public bool save = false;
    //bool firstTime = true;
    void Start()
    {
        carInput = GetComponent<CarInput>();
        distanceNNInterface = GetComponent<DistanceNNInterface>();
        videoInput = GameObject.Find("Video Input").GetComponent<VideoInput>();
#if !oldway // Old way to save traning data
        using (StreamWriter sw = File.AppendText(filename))
        {
            sw.WriteLine("steering,acc,LK,RK");
        }
#endif
        
    }



    // Update is called once per frame


    

    void Update()
    {
        if (recordData)
        {
            if ( !(float.IsNaN(videoInput.leftTilt) 
                || float.IsNaN(videoInput.rightTilt)
                || float.IsInfinity(videoInput.leftTilt)
                || float.IsInfinity(videoInput.rightTilt))
                ) {


                TrainingData data = new TrainingData(2, 1);

                float steering = distanceNNInterface.algorithmicSteering;

                float steerLeft = steering < 0 ? -steering : 0;
                float steerRight = steering > 0 ? steering : 0;    // [ 0, 1]



                data.inputs[(int)TrainingData.Side.left] = videoInput.leftTilt;
                data.inputs[(int)TrainingData.Side.right] = videoInput.rightTilt;
                //data.outputs[(int)TrainingData.Side.left] = steerLeft;
                //data.outputs[(int)TrainingData.Side.right] = steerRight;

                data.outputs[0] = (carInput.steering + 1f)/2f;

                trainingData.Add(data);
            }
            
        }
        if (save)
        {
            recordData = false;
            save = false;
            /*
            var turns = TrainingData.CategorizeByTurn(trainingData);

            int superLefts = turns[0].Count;
            int lefts = turns[1].Count;
            int straights = turns[2].Count;
            int rights = turns[3].Count;
            int superRights = turns[4].Count;


            Debug.Log($"{superLefts}\n{lefts}\n{straights}\n{rights}\n{superRights}");

            int min = Mathf.Min(new int[] { superLefts, lefts, straights, rights, superRights });

            var trimmedData = turns[0].GetRange(0, min);
            trimmedData.AddRange(turns[1].GetRange(0, min));
            trimmedData.AddRange(turns[2].GetRange(0, min));
            trimmedData.AddRange(turns[3].GetRange(0, min));
            trimmedData.AddRange(turns[4].GetRange(0, min));
            
            Debug.Log($"saving file with {trimmedData.Count} lines");
            */

            TrainingData.SaveToFile(trainingData, filename);
        }



#if !oldway // Old way to save traning data
            using (StreamWriter sw = File.AppendText(filename))
            {
                if (firstTime)
                {
                    sw.WriteLine("FW$BW$SL$SR$LK$RK");
                    firstTime = false;
                }
                //sw.WriteLine($"{carInput.steering}${carInput.acceleration}${videoInput.leftTilt}${videoInput.rightTilt}");
                sw.WriteLine($"{carInput.forward}${carInput.backward}${carInput.steerLeft}${carInput.steerRight}${videoInput.leftTilt}${videoInput.rightTilt}");
            }

        }
#endif
    }
}
