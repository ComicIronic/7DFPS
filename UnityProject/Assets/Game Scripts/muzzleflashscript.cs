using UnityEngine;
using System.Collections;

public class muzzleflashscript : taser_spark
{
	void Update() {
		UpdateColor();
		opac -= Time.deltaTime * 50.0f;
		if(opac <= 0.0f){
			Destroy(gameObject);
		}
	}
}

