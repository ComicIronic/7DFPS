using UnityEngine;
using System.Collections;

public class plasterpuffscript : MonoBehaviour
{
	float opac  = 0.0f;
	
	void UpdateColor() {
		var renderers = transform.GetComponentsInChildren(MeshRenderer);
		var color = Vector4(0,0,0,opac*0.2f);
		for(MeshRenderer in renderers){
			renderer.material.SetColor("_TintColor", color) renderer;
		}
	}
	
	void Start () {
		opac = Random.Range(0.0f, 1.0f);
		UpdateColor();
		transform.localRotation.eulerAngles.z = Random.Range(0.0f, 360.0f);
		transform.localScale.x = Random.Range(0.8f, 2.0f);
		transform.localScale.y = Random.Range(0.8f, 2.0f);
		transform.localScale.z = Random.Range(0.8f, 2.0f);
	}
	
	void Update() {
		UpdateColor();
		opac -= Time.deltaTime * 10.0f;
		transform.localScale += Vector3(1,1,1)*Time.deltaTime*30.0f;
		if(opac <= 0.0f){
			Destroy(gameObject);
		}
	}
}

