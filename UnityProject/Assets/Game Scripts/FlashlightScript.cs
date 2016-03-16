using UnityEngine;
using System.Collections;

public class FlashlightScript : MonoBehaviour
{
	AnimationCurve battery_curve;
	AudioClip sound_turn_on;
	AudioClip sound_turn_off;
	private float kSoundVolume  = 0.3f;
	private bool switch_on  = false;
	private static float max_battery_life  = 60*60*5.5f;
	private float battery_life_remaining = max_battery_life;
	
	private float initial_pointlight_intensity;
	private float initial_spotlight_intensity;
	
	void Awake() {
		switch_on = false;// Random.Range(0.0f, 1.0f) < 0.5f;
	}
	
	void Start () {
		initial_pointlight_intensity = transform.FindChild("Pointlight").gameObject.GetComponent<Light>().intensity;
		initial_spotlight_intensity = transform.FindChild("Spotlight").gameObject.GetComponent<Light>().intensity;
		battery_life_remaining = Random.Range(max_battery_life*0.2f, max_battery_life);
	}
	
	public void TurnOn() {
		if(!switch_on){
			switch_on = true;
			audio.PlayOneShot(sound_turn_on, kSoundVolume * PlayerPrefs.GetFloat("sound_volume", 1.0f));
		}
	}
	
	public void TurnOff() {
		if(switch_on){
			switch_on = false;
			audio.PlayOneShot(sound_turn_off, kSoundVolume * PlayerPrefs.GetFloat("sound_volume", 1.0f));
		}
	}
	
	void Update () {
		if(switch_on){
			battery_life_remaining -= Time.deltaTime;
			if(battery_life_remaining <= 0.0f){
				battery_life_remaining = 0.0f;
			}
			var battery_curve_eval = battery_curve.Evaluate(1.0f-battery_life_remaining/max_battery_life);
			transform.FindChild("Pointlight").gameObject.GetComponent<Light>().intensity = initial_pointlight_intensity * battery_curve_eval * 8.0f;
			transform.FindChild("Spotlight").gameObject.GetComponent<Light>().intensity = initial_spotlight_intensity * battery_curve_eval * 3.0f;
			transform.FindChild("Pointlight").gameObject.GetComponent<Light>().enabled = true;
			transform.FindChild("Spotlight").gameObject.GetComponent<Light>().enabled = true;
		} else {
			transform.FindChild("Pointlight").gameObject.GetComponent<Light>().enabled = false;
			transform.FindChild("Spotlight").gameObject.GetComponent<Light>().enabled = false;
		}
		if(rigidbody){
			transform.FindChild("Pointlight").light.enabled = true;
			transform.FindChild("Pointlight").light.intensity = 1.0f + Mathf.Sin(Time.time * 2.0f);
			transform.FindChild("Pointlight").light.range = 1.0f;
		} else {
			transform.FindChild("Pointlight").light.range = 10.0f;
		}
	}
}

