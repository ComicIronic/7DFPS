using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour
{
	AudioClip[] sound_hit_concrete;
	AudioClip[] sound_hit_metal;
	AudioClip[] sound_hit_glass;
	AudioClip[] sound_hit_body;
	AudioClip[] sound_hit_ricochet;
	AudioClip[] sound_glass_break;
	AudioClip[] sound_flyby;
	GameObject bullet_obj;
	GameObject bullet_hole_obj;
	GameObject glass_bullet_hole_obj;
	GameObject metal_bullet_hole_obj;
	GameObject spark_effect;
	GameObject puff_effect;
	private Vector3 old_pos;
	private bool hit_something = false;
	private LineRenderer line_renderer; 
	private Vector3 velocity;
	private float life_time = 0.0f;
	private float death_time = 0.0f;
	private int segment = 1;
	private bool hostile = false;
	
	void SetVelocity(Vector3 vel){
		this.velocity = vel;
	}
	
	void SetHostile() {
		audio.rolloffMode = AudioRolloffMode.Logarithmic;
		this.PlaySoundFromGroup(sound_flyby, 0.4f);
		hostile = true;
	}
	
	void Start () {
		line_renderer = GetComponent<LineRenderer>();
		line_renderer.SetPosition(0, transform.position);
		line_renderer.SetPosition(1, transform.position);
		old_pos = transform.position;
	}
	
	void Update () {
		if(!hit_something){
			life_time += Time.deltaTime;
			if(life_time > 1.5f){
				hit_something = true;
			}
			transform.position += velocity * Time.deltaTime;
			velocity += Physics.gravity * Time.deltaTime;
			RaycastHit hit;
			if(Physics.Linecast(old_pos, transform.position, out hit, 1<<0 | 1<<9 | 1<<11)){
				var hit_obj = hit.collider.gameObject;
				var hit_transform_obj = hit.transform.gameObject;
				ShootableLight light_script = (ShootableLight)Tools.RecursiveHasScript(hit_obj, "ShootableLight", 1);
				AimScript aim_script = (AimScript)Tools.RecursiveHasScript(hit_obj, "AimScript", 1);
				RobotScript turret_script = (RobotScript)Tools.RecursiveHasScript(hit_obj, "RobotScript", 3);
				transform.position = hit.point;
				var ricochet_amount = Vector3.Dot(velocity.normalized, hit.normal) * -1.0f;
				if(Random.Range(0.0f, 1.0f) > ricochet_amount && Vector3.Magnitude(velocity) * (1.0f-ricochet_amount) > 10.0f){
					GameObject ricochet = (GameObject)Instantiate(bullet_obj, hit.point, transform.rotation);
					var ricochet_vel = velocity * 0.3f * (1.0f-ricochet_amount);
					velocity -= ricochet_vel;
					ricochet_vel = Vector3.Reflect(ricochet_vel, hit.normal);
					ricochet.GetComponent<BulletScript>().SetVelocity(ricochet_vel);
					this.PlaySoundFromGroup(sound_hit_ricochet, hostile ? 1.0f : 0.6f);
				} else if(turret_script && velocity.magnitude > 100.0f){
					RaycastHit new_hit;
					if(Physics.Linecast(hit.point + velocity.normalized * 0.001f, hit.point + velocity.normalized, out new_hit, 1<<11 | 1<<12)){
						if(new_hit.collider.gameObject.layer == 12){
							turret_script.WasShotInternal(new_hit.collider.gameObject);
						}
					}					
				}
				if(hit_transform_obj.rigidbody){
					hit_transform_obj.rigidbody.AddForceAtPosition(velocity * 0.01f, hit.point, ForceMode.Impulse);
				}
				if(light_script){
					light_script.WasShot(hit_obj, hit.point, velocity);
					if(hit.collider.material.name == "glass (Instance)"){
						this.PlaySoundFromGroup(sound_glass_break, 1.0f);
					}
				}
				if(Vector3.Magnitude(velocity) > 50){
					GameObject hole;
					GameObject effect;
					if(turret_script){
						this.PlaySoundFromGroup(sound_hit_metal, hostile ? 1.0f : 0.8f);
						hole = (GameObject)Instantiate(metal_bullet_hole_obj, hit.point, Tools.RandomOrientation());
						effect = (GameObject)Instantiate(spark_effect, hit.point, Tools.RandomOrientation());
						turret_script.WasShot(hit_obj, hit.point, velocity);
					} else if(aim_script){
						hole = (GameObject)Instantiate(bullet_hole_obj, hit.point, Tools.RandomOrientation());
						effect = (GameObject)Instantiate(puff_effect, hit.point, Tools.RandomOrientation());
						this.PlaySoundFromGroup(sound_hit_body, 1.0f);
						aim_script.WasShot();
					} else if(hit.collider.material.name == "metal (Instance)"){
						this.PlaySoundFromGroup(sound_hit_metal, hostile ? 1.0f : 0.4f);
						hole = (GameObject)Instantiate(metal_bullet_hole_obj, hit.point, Tools.RandomOrientation());
						effect = (GameObject)Instantiate(spark_effect, hit.point, Tools.RandomOrientation());
					} else if(hit.collider.material.name == "glass (Instance)"){
						this.PlaySoundFromGroup(sound_hit_glass, hostile ? 1.0f : 0.4f);
						hole = (GameObject)Instantiate(glass_bullet_hole_obj, hit.point, Tools.RandomOrientation());
						effect = (GameObject)Instantiate(spark_effect, hit.point, Tools.RandomOrientation());
					} else {
						this.PlaySoundFromGroup(sound_hit_concrete, hostile ? 1.0f : 0.4f);
						hole = (GameObject)Instantiate(bullet_hole_obj, hit.point, Tools.RandomOrientation());
						effect = (GameObject)Instantiate(puff_effect, hit.point, Tools.RandomOrientation());
					}
					effect.transform.position += hit.normal * 0.05f;
					hole.transform.position += hit.normal * 0.01f;
					if(!aim_script){
						hole.transform.parent = hit_obj.transform;
					} else {
						hole.transform.parent = GameObject.Find("Main Camera").transform;
					}
				}
				hit_something = true;
			}
			line_renderer.SetVertexCount(segment+1);
			line_renderer.SetPosition(segment, transform.position);
			++segment;
		} else {
			life_time += Time.deltaTime;
			death_time += Time.deltaTime;
			//Destroy(this.gameObject);
		}
		for(int i = 0; i<segment; ++i){
			var start_color = new Color(1,1,1,(1.0f - life_time * 5.0f)*0.05f);
			var end_color = new Color(1,1,1,(1.0f - death_time * 5.0f)*0.05f);
			line_renderer.SetColors(start_color, end_color);
			if(death_time > 1.0f){
				Destroy(this.gameObject);
			}
		}
	}
}

