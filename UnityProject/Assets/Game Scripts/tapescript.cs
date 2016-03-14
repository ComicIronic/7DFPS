using UnityEngine;
using System.Collections;

public class tapescript : MonoBehaviour
{
	private float life_time  = 0.0f;
	private Vector3 old_pos;
	
	void Start () {
		old_pos = transform.position;
	}
	
	void Update () {
		transform.FindChild("light_obj").light.intensity = 1.0f + Mathf.Sin(Time.time * 2.0f);
	}
	
	void FixedUpdate() {
		if(rigidbody && !rigidbody.IsSleeping() && collider && collider.enabled){
			life_time += Time.deltaTime;
			RaycastHit hit;
			if(Physics.Linecast(old_pos, transform.position, hit, 1)){
				transform.position = hit.point;
				transform.rigidbody.velocity *= -0.3f;
			}
			if(life_time > 2.0f){
				rigidbody.Sleep();
			}
		}
		old_pos = transform.position;
	}
}

