using UnityEngine;
using System.Collections;

public class ShootableLight : MonoBehaviour
{
	
	GameObject destroy_effect;
	Color light_color = Color(1,1,1);
	bool destroyed  = false;
	public enum LightType {AIRPLANE_BLINK, NORMAL, FLICKER}
	public LightType light_type = LightType.NORMAL;
	private float blink_delay  = 0.0f;
	
	private float light_amount  = 1.0f;
	
	public void WasShot(GameObject obj, Vector3 pos, Vector3 vel) {
		if(!destroyed){
			destroyed = true;
			light_amount = 0.0f;
			(GameObject)Instantiate(destroy_effect, transform.FindChild("bulb").position, Quaternion.identity);
		}
		if(obj && obj.collider && obj.collider.material.name == "glass (Instance)"){
			GameObject.Destroy(obj);
		}
	}
	
	void Start () {
		
	}
	
	void Update () {
		if(!destroyed){
			switch(light_type){
			case LightType.AIRPLANE_BLINK:
				if(blink_delay <= 0.0f){
					blink_delay = 1.0f;
					if(light_amount == 1.0f){
						light_amount = 0.0f;
					} else {
						light_amount = 1.0f;
					}
				}
				blink_delay -= Time.deltaTime;
				break;
			}
		}
		
		var combined_color = Color(light_color.r * light_amount,light_color.g * light_amount,light_color.b * light_amount);
		for(Light in gameObject.GetComponentsInChildren(Light)){
			light.color  light= combined_color;
		}
		for(MeshRenderer in gameObject.GetComponentsInChildren(MeshRenderer)){
			renderer.material.SetColor("_Illum", combined_color) renderer;
			if(renderer.gameObject.name == "shade"){
				renderer.material.SetColor("_Illum", combined_color * 0.5f);
			}
		}
	}
}

