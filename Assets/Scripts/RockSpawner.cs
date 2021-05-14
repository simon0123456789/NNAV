using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class RockSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameObject rockPrefab;
    private static GameObject[] rocks = new GameObject[934];
    static int rockCounter = 0;
    public static bool crazyStonesHasBeenGenerated = false;

    public IEnumerator CrazyStonesCoRoutine()
    {
        const int steps = 20;
        int i = 0;
        for (int x = -89; x < 400; x += steps)
        {
            for (int z = -265; z < 180; z += steps)
            {
                rocks[i++] = Instantiate(rockPrefab, new Vector3(x, 0, z), Quaternion.identity);
                rockCounter++;
            }
        }
        Debug.Log("Done Generating Stones! Waiting for stones to fall down...");
        yield return new WaitForSeconds(10);
        Debug.Log("Done waiting... Optimzing stones");
        RemoveOffMapStones();
        Debug.Log("Done optimizing stones!");
    }

    public void GenerateCrazyStones()
    {
        StartCoroutine(CrazyStonesCoRoutine());
        return;
        
    }

    public void RemoveAllStones()
    {
        for (int i = 0; i < rockCounter; i++)
            if (rocks[i] != null)
                Destroy(rocks[i]);
        rockCounter = 0;
    }

    private void RemoveOffMapStones()
    {
        for (int i = 0; i < rockCounter; i++)
            if (rocks[i] != null)
                if (rocks[i].transform.position.y < -15)
                    Destroy(rocks[i]);
                else
                    rocks[i].GetComponent<Rigidbody>().isKinematic = true;

        bool destroyedLastStone = false;
        for (int i = 0; i < rockCounter; i++)
        {
            if (rocks[i] != null)
            {
                if (destroyedLastStone)
                    destroyedLastStone = false;
                else
                {
                    destroyedLastStone = true;
                    Destroy(rocks[i]);
                }
            }
        }
            
    }
}
