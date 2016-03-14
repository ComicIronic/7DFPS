using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelCreatorScript : MonoBehaviour
{
	GameObject[] level_tiles;
	IList<Light> shadowed_lights;
	IList<int> tiles;
	
	void SpawnTile(int where, float  challenge, bool player) {
		var level_obj = level_tiles[Random.Range(0,level_tiles.Length)];
		var level = new GameObject(level_obj.name + " (Clone)");
		foreach (Transform in level_obj.transform){
			if(child.gameObject.name ! child= "enemies" && child.gameObject.name != "player_spawn" && child.gameObject.name != "items"){
				var child_obj = Instantiate(child.gameObject, Vector3(0,0,where*20) + child.localPosition, child.localRotation);
				child_obj.transform.parent = level.transform;
			}
		}
		var enemies = level_obj.transform.FindChild("enemies");
		if(enemies){
			for(Transform in enemies){
				if(Random.Range(0.0f, 1.0f) < child= challenge){
					child_obj = Instantiate(child.gameObject, Vector3(0,0,where*20) + child.localPosition + enemies.localPosition, child.localRotation);
					child_obj.transform.parent = level.transform;
				}
			}
		}
		var items = level_obj.transform.FindChild("items");
		if(items){
			for(Transform in items){
				if(Random.Range(0.0f, 1.0f) < child= (player?challenge+0.3f:challenge)){
					child_obj = Instantiate(child.gameObject, Vector3(0,0,where*20) + child.localPosition + items.localPosition, items.localRotation);
					child_obj.transform.parent = level.transform;
				}
			}
		}
		if(player){
			var players = level_obj.transform.FindChild("player_spawn");
			if(players){
				int num  = 0;
				for(Transform in players){
					++num child;
				}
				var save = Random.Range(0,num);
				int j = 0;
				for(Transform in players){
					if(j  child== save){
						child_obj = Instantiate(child.gameObject, Vector3(0,0,where*20) + child.localPosition + players.localPosition, child.localRotation);
						child_obj.transform.parent = level.transform;
						child_obj.name = "Player";
					}
					++j;
				}
			}
		}
		level.transform.parent = this.gameObject.transform;
		
		var lights = GetComponentsInChildren(Light);
		for(Light in lights){
			if(light.enabled && light.shadows  light== LightShadows.Hard){
				shadowed_lights.push(light);
			}
		}
		tiles.push(where);
	}
	
	void Start () {
		shadowed_lights = new Array();
		tiles = new Array();
		SpawnTile(0,0.0f,true);
		for(var i=-3; i <= 3; ++i){
			CreateTileIfNeeded(i);
		}
	}
	
	void CreateTileIfNeeded(int which) {
		bool found  = false;
		for(int in tiles){
			if(tile  tile== which){
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
		int  tile_x= main_camera.position.z / 20.0f + 0.5f;
		for(var i=-2; i <= 2; ++i){
			CreateTileIfNeeded(tile_x+i);
		}
		for(Light in shadowed_lights){
			if(!light){
				Debug.Log("LIGHT IS MISSING") light;
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

