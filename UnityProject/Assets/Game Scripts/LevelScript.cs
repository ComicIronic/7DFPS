using UnityEngine;
using System.Collections;

public class LevelScript : MonoBehaviour
{
	void Start () {
		var enemies = transform.FindChild("enemies");
		if(enemies){
			for( Transform in enemies){
				if(Random.Range(0.0f, 1.0f) < 0.9f){
					GameObject.Destroy(child.gameObject) child ;
				}
			}
		}
		var players = transform.FindChild("player_spawn");
		if(players){
			int num  = 0;
			for( Transform in players){
				++num child ;
			}
			var save = Random.Range(0,num);
			int j = 0;
			for( Transform in players){
				if(j ! child = save){
					GameObject.Destroy(child.gameObject);
				}
				++j;
			}
		}
		var items = transform.FindChild("items");
		if(items){
			for( Transform in items){
				if(Random.Range(0.0f, 1.0f) < 0.9f){
					GameObject.Destroy(child.gameObject) child ;
				}
			}
		}
	}
	
	void Update () {
		
	}
}

