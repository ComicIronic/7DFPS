using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelCreatorScript : MonoBehaviour
{
	public GameObject[] level_tiles;
	List<Light> shadowed_lights;
	List<int> tiles;
	
	void SpawnTile(int where, float  challenge, bool player) {
		var level_obj = level_tiles[Random.Range(0,level_tiles.Length)];
		var level = new GameObject(level_obj.name + " (Clone)");
		foreach (Transform child in level_obj.transform){
			if(child.gameObject.name != "enemies" && child.gameObject.name != "player_spawn" && child.gameObject.name != "items"){
				GameObject child_obj = (GameObject)Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition, child.localRotation);
				child_obj.transform.parent = level.transform;
			}
		}
		var enemies = level_obj.transform.Find("enemies");
		if(enemies){
			foreach(Transform child in enemies){
				if(Random.Range(0.0f, 1.0f) < challenge){
					GameObject child_obj = (GameObject)Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition + enemies.localPosition, child.localRotation);
					child_obj.transform.parent = level.transform;
				}
			}
		}
		var items = level_obj.transform.Find("items");
		if(items){
			foreach(Transform child in items){
				if(Random.Range(0.0f, 1.0f) < (player?challenge+0.3f:challenge)){
					GameObject child_obj = (GameObject)Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition + items.localPosition, items.localRotation);
					child_obj.transform.parent = level.transform;
				}
			}
		}
		if(player){
			var players = level_obj.transform.Find("player_spawn");
			if(players){
				int num  = 0;
				foreach(Transform child in players){
					++num;
				}
				var save = Random.Range(0,num);
				int j = 0;
				foreach(Transform child in players){
					if(j == save){
						GameObject child_obj = (GameObject)Instantiate(child.gameObject, new Vector3(0,0,where*20) + child.localPosition + players.localPosition, child.localRotation);
						child_obj.transform.parent = level.transform;
						child_obj.name = "Player";
					}
					++j;
				}
			}
		}
		level.transform.parent = this.gameObject.transform;
		
		var lights = GetComponentsInChildren<Light>();
		foreach(Light light in lights){
			if(light.enabled && light.shadows == LightShadows.Hard){
				shadowed_lights.Add(light);
			}
		}
		tiles.Add(where);
	}
	
	void Start () {
		shadowed_lights = new List<Light> ();
		tiles = new List<int> ();
		SpawnTile(0,0.0f,true);
		for(var i=-3; i <= 3; ++i){
			CreateTileIfNeeded(i);
		}
	}
	
	void CreateTileIfNeeded(int which) {
		bool found  = false;
		foreach(int tile in tiles){
			if(tile == which){
				found = true;
			}
		}
		if(!found){
			//Debug.Log("Spawning tile: "+which);
			SpawnTile(which, Mathf.Min(0.6f,0.1f * Mathf.Abs(which)), false);
		}
	}
	
	
	void Update () {
		var main_camera = GameObject.Find("Main Camera").transform;
		int  tile_x = (int)( main_camera.position.z / 20.0f + 0.5f);
		for(var i=-2; i <= 2; ++i){
			CreateTileIfNeeded(tile_x+i);
		}
		foreach(Light light in shadowed_lights){
			if(!light){
				Debug.Log("LIGHT IS MISSING");
			}
			if(light){
				var shadowed_amount = Vector3.Distance(main_camera.position, light.gameObject.transform.position);
				var shadow_threshold = Mathf.Min(30,light.range*2.0f);
				var fade_threshold = shadow_threshold * 0.75f;
				if(shadowed_amount < shadow_threshold){
					light.shadows = LightShadows.Hard;
					light.shadowStrength = Mathf.Min(1.0f, 1.0f-(fade_threshold - shadowed_amount) / (fade_threshold - shadow_threshold));
				} else {
					light.shadows = LightShadows.None;
				}
			}
		}
	}
}

