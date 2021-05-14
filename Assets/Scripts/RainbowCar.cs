using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowCar : MonoBehaviour
{
    // Start is called before the first frame update
    Material carMaterial;
	
    void Start()
    {
        GameObject gameObject = GameObject.Find("SportCar20_Paint_LOD0");
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		carMaterial = meshRenderer.materials[0];
		carMaterial.color = Color.green;

		GameObject licensePlate = GameObject.Find("SportCar20_NumberTypeEU");
		//Debug.Log(licensePlate.GetComponent<MeshRenderer>().materials[0]);
		licensePlate.GetComponent<MeshRenderer>().materials[0] = (Material)Resources.Load("Eu_Number_Z", typeof(Material));
	}

	// Update is called once per frame

	float r = 0, g = 44, b = 43;
    void Update()
    {
		if (r > 0 && b == 0)
		{
			r--;
			g++;
		}
		if (g > 0 && r == 0)
		{
			g--;
			b++;
		}
		if (b > 0 && g == 0)
		{
			r++;
			b--;
		}
		carMaterial.color = new Color(r / 100f, g / 100f, b / 100f);

		
	}
}
