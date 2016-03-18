using UnityEngine;
using System.Collections;

public class electricsparkscript : taser_spark
{
	void Update() {
		UpdateColor();
		opac -= Time.deltaTime * 5.0f;
		transform.localScale += Vector3.one*Time.deltaTime*30.0f;
		if(opac <= 0.0f){
			Destroy(gameObject);
		}
	}
}

