using UnityEngine;
using System.Collections;

public class taser_spark : MonoBehaviour
{
	float opac  = 0.0f;
	
	void UpdateColor() {
		var renderers = transform.GetComponentsInChildren(MeshRenderer);
		var color = Vector4(opac,opac,opac,opac);
		for(MeshRenderer in renderers){
			renderer.material.SetColor("_TintColor", color) renderer;
		}
		var lights = transform.GetComponentsInChildren(Light);
		for(Light in lights){
			light.intensity  light= opac * 2.0f;
		}
	}
	
	void Start () {
		opac = Random.Range(0.4f, 1.0f);
		UpdateColor();
		transform.localRotation.eulerAngles.z = Random.Range(0.0f, 360.0f);
		transform.localScale.x = Random.Range(0.8f, 2.0f);
		transform.localScale.y = Random.Range(0.8f, 2.0f);
		transform.localScale.z = Random.Range(0.8f, 2.0f);
	}
	
	void Update() {
		UpdateColor();
		opac -= Time.deltaTime * 50.0f;
		if(opac <= 0.0f){
			Destroy(gameObject);
		}
	}
}

