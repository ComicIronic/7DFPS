using UnityEngine;
using System.Collections;

public class mag_script : MonoBehaviour
{
	public enum MagLoadStage {NONE, PUSHING_DOWN, ADDING_ROUND, REMOVING_ROUND, PUSHING_UP};

	private int num_rounds = 8;
	public int kMaxRounds = 8;
	private Vector3[] round_pos;
	private Quaternion[] round_rot;
	private Vector3 old_pos;
	public Vector3 hold_offset;
	public Vector3 hold_rotation;
	bool collided = false;
	public AudioClip[] sound_add_round;
	public AudioClip[] sound_mag_bounce;
	float life_time = 0.0f;

	MagLoadStage mag_load_stage = MagLoadStage.NONE;
	float mag_load_progress = 0.0f;
	bool disable_interp = true;
	
private void Start () {
		old_pos = transform.position;
		num_rounds = Random.Range (0, kMaxRounds);

		//Initialise the arrays for holding rounds
		round_pos = new Vector3[kMaxRounds];
		round_rot = new Quaternion[kMaxRounds];

		for (int i = 0; i<kMaxRounds; ++i) {
			//Locates where rounds go from the model
			var round = transform.Find ("round_" + (i + 1));
			round_pos [i] = round.localPosition;
			round_rot [i] = round.localRotation;
			if (i < num_rounds) {
				round.GetComponent<Renderer>().enabled = true;
			} else {
				//TODO Find out why this is false? why not render the last bullet?
				round.GetComponent<Renderer>().enabled = false;
			}
		}
	}

void Update() {
			switch (mag_load_stage) {
			case MagLoadStage.PUSHING_DOWN:
					mag_load_progress += Time.deltaTime * 20.0f;
					if (mag_load_progress >= 1.0f) {
							mag_load_stage = MagLoadStage.ADDING_ROUND;
							mag_load_progress = 0.0f;
					}
					break;
			case MagLoadStage.ADDING_ROUND:
					mag_load_progress += Time.deltaTime * 20.0f;
					if (mag_load_progress >= 1.0f) {
							mag_load_stage = MagLoadStage.NONE;
							mag_load_progress = 0.0f;
							for (int i = 0; i<num_rounds; ++i) {
					Transform obj = transform.Find ("round_" + (i + 1));
									obj.localPosition = round_pos [i];
									obj.localRotation = round_rot [i];
							}
					}
					break;
			case MagLoadStage.PUSHING_UP:
					mag_load_progress += Time.deltaTime * 20.0f;
					if (mag_load_progress >= 1.0f) {
							mag_load_stage = MagLoadStage.NONE;
							mag_load_progress = 0.0f;
							RemoveRound ();
							for(int i=0; i<num_rounds; ++i) {
					Transform obj = transform.Find ("round_" + (i + 1));
									obj.localPosition = round_pos [i];
									obj.localRotation = round_rot [i];
							}
					}
					break;
			case MagLoadStage.REMOVING_ROUND:
					mag_load_progress += Time.deltaTime * 20.0f;
					if (mag_load_progress >= 1.0f) {
							mag_load_stage = MagLoadStage.PUSHING_UP;
							mag_load_progress = 0.0f;
					}
					break;
			}
			var mag_load_progress_display = mag_load_progress;
			if (disable_interp) {
					mag_load_progress_display = Mathf.Floor (mag_load_progress + 0.5f);
			}
			switch (mag_load_stage) {
			case MagLoadStage.PUSHING_DOWN:
					Transform obj = gameObject.transform.Find ("round_1");
					obj.transform.localPosition = Vector3.Lerp (transform.Find ("point_start_load").localPosition, 
		                                 transform.Find ("point_load").localPosition, 
		                                 mag_load_progress_display);
					obj.transform.localRotation = Quaternion.Slerp (transform.Find ("point_start_load").localRotation, 
		                                     transform.Find ("point_load").localRotation, 
		                                     mag_load_progress_display);
					for(int i=1; i<num_rounds; ++i) {
							obj = transform.Find ("round_" + (i + 1));
							obj.transform.localPosition = Vector3.Lerp (round_pos [i - 1], round_pos [i], mag_load_progress_display);
							obj.transform.localRotation = Quaternion.Slerp (round_rot [i - 1], round_rot [i], mag_load_progress_display);
					}
					break;
			case MagLoadStage.ADDING_ROUND:
					obj = transform.Find ("round_1");
					obj.localPosition = Vector3.Lerp (transform.Find ("point_load").localPosition, 
		                                 round_pos [0], 
		                                 mag_load_progress_display);
					obj.localRotation = Quaternion.Slerp (transform.Find ("point_load").localRotation, 
		                                     round_rot [0], 
		                                     mag_load_progress_display);
					for(int i=1; i<num_rounds; ++i) {
							obj = transform.Find ("round_" + (i + 1));
							obj.localPosition = round_pos [i];
					}
					break;
			case MagLoadStage.PUSHING_UP:
					obj = transform.Find ("round_1");
					obj.localPosition = Vector3.Lerp (transform.Find ("point_start_load").localPosition, 
		                                 transform.Find ("point_load").localPosition, 
		                                 1.0f - mag_load_progress_display);
					obj.localRotation = Quaternion.Slerp (transform.Find ("point_start_load").localRotation, 
		                                     transform.Find ("point_load").localRotation, 
		                                     1.0f - mag_load_progress_display);
					for(int i=1; i<num_rounds; ++i) {
							obj = transform.Find ("round_" + (i + 1));
							obj.localPosition = Vector3.Lerp (round_pos [i - 1], round_pos [i], mag_load_progress_display);
							obj.localRotation = Quaternion.Slerp (round_rot [i - 1], round_rot [i], mag_load_progress_display);
					}
					break;
			case MagLoadStage.REMOVING_ROUND:
					obj = transform.Find ("round_1");
					obj.localPosition = Vector3.Lerp (transform.Find ("point_load").localPosition, 
		                                 round_pos [0], 
		                                 1.0f - mag_load_progress_display);
					obj.localRotation = Quaternion.Slerp (transform.Find ("point_load").localRotation, 
		                                     round_rot [0], 
		                                     1.0f - mag_load_progress_display);
					for(int i=1; i<num_rounds; ++i) {
							obj = transform.Find ("round_" + (i + 1));
							obj.localPosition = round_pos [i];
							obj.localRotation = round_rot [i];
					}
					break;
			}
	}

public bool RemoveRound() {
			if (num_rounds == 0) {
					return false;
			}
			var round_obj = transform.Find ("round_" + num_rounds);
			round_obj.GetComponent<Renderer>().enabled = false;
			num_rounds--;
			return true;
	}

public bool RemoveRoundAnimated() {
			if (num_rounds == 0 || mag_load_stage != MagLoadStage.NONE) {
					return false;
			}
			mag_load_stage = MagLoadStage.REMOVING_ROUND;
			mag_load_progress = 0.0f;
			return true;
	}

public bool IsFull() {
			return num_rounds == kMaxRounds;
	}

public bool AddRound() {
			if (num_rounds >= kMaxRounds || mag_load_stage != MagLoadStage.NONE) {
					return false;
			}
			mag_load_stage = MagLoadStage.PUSHING_DOWN;
			mag_load_progress = 0.0f;
			this.PlaySoundFromGroup (sound_add_round, 0.3f);
			++num_rounds;
			var round_obj = transform.Find ("round_" + num_rounds);
			round_obj.GetComponent<Renderer>().enabled = true;
			return true;
	}

public int NumRounds() {
			return num_rounds;
	}

void CollisionSound() {
			if (!collided) {
					collided = true;
					this.PlaySoundFromGroup (sound_mag_bounce, 0.3f);
			}
	}

void FixedUpdate () {
    Rigidbody rigidbody = GetComponent<Rigidbody>();
    Collider collider = GetComponent<Collider>();
		if (rigidbody && !GetComponent<Rigidbody>().IsSleeping () && collider && collider.enabled) {
			life_time += Time.deltaTime;
			RaycastHit hit;
			if (Physics.Linecast (old_pos, transform.position, out hit, 1)) {
				transform.position = hit.point;
				transform.GetComponent<Rigidbody>().velocity *= -0.3f;
			}
			if (life_time > 2.0f) {
				GetComponent<Rigidbody>().Sleep ();
			}
		} else if (!rigidbody) {
			life_time = 0.0f;
			collided = false;
		}
		old_pos = transform.position;
	}

void OnCollisionEnter (Collision collision) {
		CollisionSound ();
	}
}

