using UnityEngine;
using System.Collections;

public class GUISkinHolder : MonoBehaviour
{
	
	GUISkin gui_skin;
	 AudioClip[] sound_scream ;
	 AudioClip[] sound_tape_content ;
	 AudioClip sound_tape_start ;
	 AudioClip sound_tape_end ;
	 AudioClip sound_tape_background ;
	 GameObject tape_object ;
	 AudioClip win_sting ;
	 GameObject[] weapons ;
	 GameObject weapon ;
	 GameObject flashlight_object ;
	bool has_flashlight  = false;
	
	void Awake () {
		//weapon = weapons[2];
		weapon = weapons[Random.Range(0,weapons.length)];
	}
	
	void Start () {
	}
	
	void Update () {
	}
}

