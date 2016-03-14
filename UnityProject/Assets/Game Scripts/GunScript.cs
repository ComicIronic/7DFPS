using UnityEngine;
using System.Collections;

public class GunScript : MonoBehaviour
{
	enum GunType {AUTOMATIC, REVOLVER};
	GunType gun_type;
	
	enum ActionType {DOUBLE, SINGLE};
	ActionType action_type;
	
	AudioClip[] sound_gunshot_bigroom;
	AudioClip[] sound_gunshot_smallroom;
	AudioClip[] sound_gunshot_open;
	AudioClip[] sound_mag_eject_button;
	AudioClip[] sound_mag_ejection;
	AudioClip[] sound_mag_insertion;
	AudioClip[] sound_slide_back;
	AudioClip[] sound_slide_front;
	AudioClip[] sound_safety;
	AudioClip[] sound_bullet_eject;
	AudioClip[] sound_cylinder_open;
	AudioClip[] sound_cylinder_close;
	AudioClip[] sound_extractor_rod_open;
	AudioClip[] sound_extractor_rod_close;
	AudioClip[] sound_cylinder_rotate;
	AudioClip[] sound_hammer_cock;
	AudioClip[] sound_hammer_decock;
	
	private float kGunMechanicVolume  = 0.2f;
	
	bool add_head_recoil  = false;
	float recoil_transfer_x  = 0.0f;
	float recoil_transfer_y  = 0.0f;
	float rotation_transfer_x  = 0.0f;
	float rotation_transfer_y  = 0.0f;
	
	Vector3 old_pos;
	Vector3 velocity;
	
	GameObject magazine_obj;
	
	GameObject bullet_obj;
	GameObject muzzle_flash;
	
	GameObject shell_casing;
	GameObject casing_with_bullet;
	bool ready_to_remove_mag  = false;
	
	enum PressureState {NONE, INITIAL, CONTINUING};
	PressureState pressure_on_trigger = PressureState.NONE;
	float trigger_pressed  = 0.0f;
	
	private GameObject round_in_chamber;
	enum RoundState {EMPTY, READY, FIRED, LOADING, JAMMED};
	private RoundState round_in_chamber_state = RoundState.READY;
	
	private GameObject magazine_instance_in_gun;
	private float mag_offset  = 0.0f;
	
	private bool slide_pressure  = false;
	private Vector3 slide_rel_pos;
	private float slide_amount  = 0.0f;
	private bool slide_lock  = false;
	enum SlideStage {NOTHING, PULLBACK, HOLD};
	private SlideStage  slide_stage= SlideStage.NOTHING;
	
	enum Thumb{ON_HAMMER, OFF_HAMMER, SLOW_LOWERING};
	private Thumb thumb_on_hammer = Thumb.OFF_HAMMER;
	private Vector3 hammer_rel_pos;
	private Quaternion hammer_rel_rot;
	private float hammer_cocked  = 1.0f;
	
	private Vector3 safety_rel_pos;
	private Quaternion safety_rel_rot;
	
	private float safety_off  = 1.0f;
	enum Safety{OFF, ON};
	private Safety safety = Safety.OFF;
	
	private float kSlideLockPosition  = 0.9f;
	private float kPressCheckPosition  = 0.4f;
	private float kSlideLockSpeed  = 20.0f;
	
	enum MagStage {OUT, INSERTING, IN, REMOVING};
	private MagStage  mag_stage= MagStage.IN;
	private float mag_seated  = 1.0f;
	
	enum AutoModStage {ENABLED, DISABLED};
	private AutoModStage  auto_mod_stage= AutoModStage.DISABLED;
	private float auto_mod_amount  = 0.0f;
	private Vector3 auto_mod_rel_pos;
	private bool fired_once_this_pull  = false;
	
	private bool has_slide  = false;
	private bool has_safety  = false;
	private bool has_hammer  = false;
	private bool has_auto_mod  = false;
	
	private Quaternion yolk_pivot_rel_rot;
	private float yolk_open  = 0.0f;
	enum YolkStage {CLOSED, OPENING, OPEN, CLOSING};
	private YolkStage  yolk_stage= YolkStage.CLOSED;
	private float cylinder_rotation  = 0.0f;
	private float cylinder_rotation_vel  = 0.0f;
	private int active_cylinder  = 0;
	private int target_cylinder_offset  = 0;
	enum ExtractorRodStage {CLOSED, OPENING, OPEN, CLOSING};
	private ExtractorRodStage extractor_rod_stage = ExtractorRodStage.CLOSED;
	private float extractor_rod_amount  = 0.0f;
	private bool extracted  = false;
	private Vector3 extractor_rod_rel_pos;
	bool disable_springs  = false;
	
	class CylinderState {
		GameObject  gameObject = null;
		bool  can_fire= false;
		float  seated= 0.0f;
		bool  falling= false;
	};
	
	private int cylinder_capacity  = 6;
	CylinderState[] cylinders;
	
	bool IsAddingRounds() {
		if(yolk_stage == YolkStage.OPEN){
			return true;
		} else {
			return false;
		}
	}
	
	bool IsEjectingRounds() {
		if(extractor_rod_stage != ExtractorRodStage.CLOSED){
			return true;
		} else {
			return false;
		}
	}
	
	Transform GetHammer() {
		var hammer = transform.FindChild("hammer");
		if(!hammer){
			hammer = transform.FindChild("hammer_pivot");
		}
		return hammer;
	}
	
	Transform GetHammerCocked() {
		var hammer = transform.FindChild("point_hammer_cocked");
		if(!hammer){
			hammer = transform.FindChild("hammer_pivot");
		}
		return hammer;
	}
	
	void Start () {
		disable_springs = false;
		if(transform.FindChild("slide")){
			var slide = transform.FindChild("slide");
			has_slide = true;
			slide_rel_pos = slide.localPosition;
			if(slide.FindChild("auto mod toggle")){
				has_auto_mod = true;
				auto_mod_rel_pos = slide.FindChild("auto mod toggle").localPosition;
				if(Random.Range(0,2) == 0){
					auto_mod_amount = 1.0f;
					auto_mod_stage = AutoModStage.ENABLED;
				}
			}
		}
		var hammer = GetHammer();
		if(hammer){
			has_hammer = true;
			hammer_rel_pos = hammer.localPosition;
			hammer_rel_rot = hammer.localRotation;
		}
		var yolk_pivot = transform.FindChild("yolk_pivot");
		if(yolk_pivot){
			yolk_pivot_rel_rot = yolk_pivot.localRotation;
			var yolk = yolk_pivot.FindChild("yolk");
			if(yolk){
				var cylinder_assembly = yolk.FindChild("cylinder_assembly");
				if(cylinder_assembly){
					var extractor_rod = cylinder_assembly.FindChild("extractor_rod");
					if(extractor_rod){
						extractor_rod_rel_pos = extractor_rod.localPosition;
					}
				}
			}
		}
		
		if(gun_type == GunType.AUTOMATIC){
			magazine_instance_in_gun = Instantiate(magazine_obj);
			magazine_instance_in_gun.transform.parent = transform;
			
			var renderers = magazine_instance_in_gun.GetComponentsInChildren(Renderer);
			for(Renderer in renderers){
				renderer.castShadows  renderer= false; 
			}
			
			if(Random.Range(0,2) == 0){
				round_in_chamber = Instantiate(casing_with_bullet, transform.FindChild("point_chambered_round").position, transform.FindChild("point_chambered_round").rotation);
				round_in_chamber.transform.parent = transform;
				round_in_chamber.transform.localScale = Vector3(1.0f,1.0f,1.0f);
				renderers = round_in_chamber.GetComponentsInChildren(Renderer);
				for(Renderer in renderers){
					renderer.castShadows  renderer= false; 
				}
			}
			
			if(Random.Range(0,2) == 0){
				slide_amount = kSlideLockPosition;
				slide_lock = true;
			}
		}
		
		if(gun_type == GunType.REVOLVER){
			cylinders = new CylinderState[cylinder_capacity];
			for(int i = 0; i<cylinder_capacity; ++i){
				cylinders[i] = new CylinderState();
				if(Random.Range(0,2) == 0){
					continue;
				}
				var name = "point_chamber_"+(i+1);
				cylinders[i].object = Instantiate(casing_with_bullet, extractor_rod.FindChild(name).position, extractor_rod.FindChild(name).rotation);
				cylinders[i].object.transform.localScale = Vector3(1.0f,1.0f,1.0f);
				cylinders[i].can_fire = true;
				cylinders[i].seated = Random.Range(0.0f, 0.5f);
				renderers = cylinders[i].object.GetComponentsInChildren(Renderer);
				for(Renderer in renderers){
					renderer.castShadows  renderer= false; 
				}
			}
		}
		
		if(Random.Range(0,2) == 0 && has_hammer){
			hammer_cocked = 0.0f;
		}
		
		if(transform.FindChild("safety")){
			has_safety = true;
			safety_rel_pos = transform.FindChild("safety").localPosition;
			safety_rel_rot = transform.FindChild("safety").localRotation;
			if(Random.Range(0,4) == 0){
				safety_off = 0.0f;
				safety = Safety.ON;
				slide_amount = 0.0f;
				slide_lock = false;
			}
		}
		
	}
	
	mag_script MagScript() {
		return magazine_instance_in_gun.GetComponent("mag_script");
	}
	
	void ShouldPullSlide() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		return (!round_in_chamber && magazine_instance_in_gun && magazine_instance_in_gun.GetComponent(mag_script).NumRounds()>0);
	}
	
	void ShouldReleaseSlideLock() {
		return (round_in_chamber && slide_lock);
	}
	
	void ShouldEjectMag() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		return (magazine_instance_in_gun && magazine_instance_in_gun.GetComponent(mag_script).NumRounds() == 0);
	}
	
	bool ChamberRoundFromMag() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		if(magazine_instance_in_gun && MagScript().NumRounds() > 0 && mag_stage == MagStage.IN){
			if(!round_in_chamber){
				MagScript().RemoveRound();
				round_in_chamber = Instantiate(casing_with_bullet, transform.FindChild("point_load_round").position, transform.FindChild("point_load_round").rotation);
				var renderers = round_in_chamber.GetComponentsInChildren(Renderer);
				for(Renderer in renderers){
					renderer.castShadows  renderer= false; 
				}
				round_in_chamber.transform.parent = transform;
				round_in_chamber.transform.localScale = Vector3(1.0f,1.0f,1.0f);
				round_in_chamber_state = RoundState.LOADING;
			}
			return true;
		} else {
			return false;
		}
	}
	
	void PullSlideBack() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		slide_amount = 1.0f;
		if(slide_lock && mag_stage == MagStage.IN && (!magazine_instance_in_gun || MagScript().NumRounds() == 0)){
			return;
		}
		slide_lock = false;
		if(round_in_chamber && (round_in_chamber_state == RoundState.FIRED || round_in_chamber_state == RoundState.READY)){
			round_in_chamber.AddComponent(Rigidbody);
			Tools.PlaySoundFromGroup(sound_bullet_eject, kGunMechanicVolume);
			round_in_chamber.transform.parent = null;
			round_in_chamber.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			round_in_chamber.rigidbody.velocity = velocity;
			round_in_chamber.rigidbody.velocity += transform.rotation * Vector3(Random.Range(2.0f, 4.0f),Random.Range(1.0f, 2.0f),Random.Range(-1.0f,-3.0f));
			round_in_chamber.rigidbody.angularVelocity = Vector3(Random.Range(-40.0f,40.0f),Random.Range(-40.0f,40.0f),Random.Range(-40.0f,40.0f));
			round_in_chamber = null;
		}
		if(!ChamberRoundFromMag() && mag_stage == MagStage.IN){
			slide_lock = true;
		}
	}
	
	void ReleaseSlideLock() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		slide_lock = false;
	}
	
	Vector3 mix( Vector3 a, Vector3 b, float  val) {
		return a + (b-a) * val;
	}
	
	Quaternion mix( Quaternion a, Quaternion b, float  val) {
		float angle  = 0.0f;
		var axis = Vector3();
		(Quaternion.Inverse(b)*a).ToAngleAxis(angle, axis);
		if(angle > 180){
			angle -= 360;
		}
		if(angle < -180){
			angle += 360;
		}
		return a * Quaternion.AngleAxis(angle * -val, axis);
	}
	
	bool ApplyPressureToTrigger() {
		if(pressure_on_trigger == PressureState.NONE){
			pressure_on_trigger = PressureState.INITIAL;
			fired_once_this_pull = false;
		} else {
			pressure_on_trigger = PressureState.CONTINUING;
		}
		if(yolk_stage != YolkStage.CLOSED){
			return;
		}
		if((pressure_on_trigger == PressureState.INITIAL || action_type == ActionType.DOUBLE) && !slide_lock && thumb_on_hammer == Thumb.OFF_HAMMER && hammer_cocked == 1.0f && safety_off == 1.0f && (auto_mod_stage == AutoModStage.ENABLED || !fired_once_this_pull)){
			trigger_pressed = 1.0f;
			if(gun_type == GunType.AUTOMATIC && slide_amount == 0.0f){
				hammer_cocked = 0.0f;
				if(round_in_chamber && round_in_chamber_state == RoundState.READY){
					fired_once_this_pull = true;
					Tools.PlaySoundFromGroup(sound_gunshot_smallroom, 1.0f);
					round_in_chamber_state = RoundState.FIRED;
					GameObject.Destroy(round_in_chamber);
					round_in_chamber = Instantiate(shell_casing, transform.FindChild("point_chambered_round").position, transform.rotation);
					round_in_chamber.transform.parent = transform;
					var renderers = round_in_chamber.GetComponentsInChildren(Renderer);
					for(Renderer in renderers){
						renderer.castShadows  renderer= false; 
					}
					
					Instantiate(muzzle_flash, transform.FindChild("point_muzzleflash").position, transform.FindChild("point_muzzleflash").rotation);
					var bullet = Instantiate(bullet_obj, transform.FindChild("point_muzzle").position, transform.FindChild("point_muzzle").rotation);
					bullet.GetComponent(BulletScript).SetVelocity(transform.forward * 251.0f);
					PullSlideBack();
					rotation_transfer_y += Random.Range(1.0f, 2.0f);
					rotation_transfer_x += Random.Range(-1.0f,1.0f);
					recoil_transfer_x -= Random.Range(150.0f, 300.0f);
					recoil_transfer_y += Random.Range(-200.0f,200.0f);
					add_head_recoil = true;
					return true;
				} else {
					Tools.PlaySoundFromGroup(sound_mag_eject_button, 0.5f);
				}
			} else if(gun_type == GunType.REVOLVER){
				hammer_cocked = 0.0f;
				var which_chamber = active_cylinder % cylinder_capacity;
				if(which_chamber < 0){
					which_chamber += cylinder_capacity;
				}
				var round = cylinders[which_chamber].object;
				if(round && cylinders[which_chamber].can_fire){
					Tools.PlaySoundFromGroup(sound_gunshot_smallroom, 1.0f);
					round_in_chamber_state = RoundState.FIRED;
					cylinders[which_chamber].can_fire = false;
					cylinders[which_chamber].seated += Random.Range(0.0f, 0.5f);
					cylinders[which_chamber].object = Instantiate(shell_casing, round.transform.position, round.transform.rotation);
					GameObject.Destroy(round);
					renderers = cylinders[which_chamber].object.GetComponentsInChildren(Renderer);
					for(Renderer in renderers){
						renderer.castShadows  renderer= false; 
					}				
					Instantiate(muzzle_flash, transform.FindChild("point_muzzleflash").position, transform.FindChild("point_muzzleflash").rotation);
					bullet = Instantiate(bullet_obj, transform.FindChild("point_muzzle").position, transform.FindChild("point_muzzle").rotation);
					bullet.GetComponent(BulletScript).SetVelocity(transform.forward * 251.0f);
					rotation_transfer_y += Random.Range(1.0f, 2.0f);
					rotation_transfer_x += Random.Range(-1.0f,1.0f);
					recoil_transfer_x -= Random.Range(150.0f, 300.0f);
					recoil_transfer_y += Random.Range(-200.0f,200.0f);
					add_head_recoil = true;
					return true;
				} else {
					Tools.PlaySoundFromGroup(sound_mag_eject_button, 0.5f);
				}
			}
		}
		
		if(action_type == ActionType.DOUBLE && trigger_pressed < 1.0f && thumb_on_hammer == Thumb.OFF_HAMMER){
			CockHammer();
			CockHammer();
		}
		
		return false;
	}
	
	void ReleasePressureFromTrigger() {
		pressure_on_trigger = PressureState.NONE;
		trigger_pressed = 0.0f;
	}
	
	bool MagEject() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		Tools.PlaySoundFromGroup(sound_mag_eject_button, kGunMechanicVolume);
		if(mag_stage != MagStage.OUT){
			mag_stage = MagStage.REMOVING;
			Tools.PlaySoundFromGroup(sound_mag_ejection, kGunMechanicVolume);
			return true;
		}
		return false;
	}
	
	void TryToReleaseSlideLock() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		if(slide_amount == kSlideLockPosition){
			ReleaseSlideLock();
		}
	}
	
	void PressureOnSlideLock() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		if(slide_amount > kPressCheckPosition && slide_stage == SlideStage.PULLBACK){
			slide_lock = true;
		} else if(slide_amount > kSlideLockPosition){// && slide_stage == SlideStage.NOTHING){
			slide_lock = true;
		}
	}
	
	void ReleasePressureOnSlideLock() {
		if(slide_amount == kPressCheckPosition){
			slide_lock = false;
			if(slide_pressure){
				slide_stage = SlideStage.PULLBACK;
			}
		} else if(slide_amount == 1.0f){
			slide_lock = false;
		}
	}
	
	void ToggleSafety() {
		if(!has_safety){
			return false;
		}
		if(safety == Safety.OFF){
			if(slide_amount == 0.0f && hammer_cocked == 1.0f){
				safety = Safety.ON;
				Tools.PlaySoundFromGroup(sound_safety, kGunMechanicVolume);
			}
		} else if(safety == Safety.ON){
			safety = Safety.OFF;
			Tools.PlaySoundFromGroup(sound_safety, kGunMechanicVolume);
		}
	}
	
	void ToggleAutoMod() {
		if(!has_auto_mod){
			return false;
		}
		Tools.PlaySoundFromGroup(sound_safety, kGunMechanicVolume);
		if(auto_mod_stage == AutoModStage.DISABLED){
			auto_mod_stage = AutoModStage.ENABLED;
		} else if(auto_mod_stage == AutoModStage.ENABLED){
			auto_mod_stage = AutoModStage.DISABLED;
		}
	}
	
	void PullBackSlide() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		if(slide_stage != SlideStage.PULLBACK && safety == Safety.OFF){
			slide_stage = SlideStage.PULLBACK;
			slide_lock = false;
		}
		slide_pressure = true;
	}
	
	void ReleaseSlide() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		slide_stage = SlideStage.NOTHING;
		slide_pressure = false;
	}
	
	void CockHammer() {
		var old_hammer_cocked = hammer_cocked;
		hammer_cocked = Mathf.Min(1.0f, hammer_cocked + Time.deltaTime * 10.0f);
		if(hammer_cocked == 1.0f && old_hammer_cocked != 1.0f){
			if(thumb_on_hammer == Thumb.ON_HAMMER){
				Tools.PlaySoundFromGroup(sound_hammer_cock, kGunMechanicVolume);
			}
			++active_cylinder;
			cylinder_rotation = active_cylinder * 360.0f / cylinder_capacity;
		}
		if(hammer_cocked < 1.0f){
			cylinder_rotation = (active_cylinder + hammer_cocked) * 360.0f / cylinder_capacity;
			target_cylinder_offset = 0.0f;
		}
	}
	
	void PressureOnHammer() {
		if(!has_hammer){
			return;
		}
		thumb_on_hammer = Thumb.ON_HAMMER;
		if(gun_type == GunType.REVOLVER && yolk_stage != YolkStage.CLOSED){
			return;
		}
		CockHammer();
	}
	
	void ReleaseHammer() {
		if(!has_hammer){
			return;
		}
		if((pressure_on_trigger != PressureState.NONE && safety_off == 1.0f) || hammer_cocked != 1.0f){
			thumb_on_hammer = Thumb.SLOW_LOWERING;
			trigger_pressed = 1.0f;
		} else {
			thumb_on_hammer = Thumb.OFF_HAMMER;
		}
	}
	
	bool IsSafetyOn() {
		return (safety == Safety.ON);
	}
	
	bool IsSlideLocked() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		return (slide_lock);
	}
	
	bool IsSlidePulledBack() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		return (slide_stage != SlideStage.NOTHING);
	}
	
	GameObject RemoveMag() {
		if(gun_type != GunType.AUTOMATIC){
			return null;
		}
		var mag = magazine_instance_in_gun;
		magazine_instance_in_gun = null;
		mag.transform.parent = null;
		ready_to_remove_mag = false;
		return mag;
	}
	
	bool IsThereAMagInGun() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		return magazine_instance_in_gun;
	}
	
	bool IsMagCurrentlyEjecting() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		return mag_stage == MagStage.REMOVING;
	}
	
	void InsertMag(GameObject mag) {
		if(gun_type != GunType.AUTOMATIC){
			return;
		}
		if(magazine_instance_in_gun){
			return;
		}
		magazine_instance_in_gun = mag;
		mag.transform.parent = transform;
		mag_stage = MagStage.INSERTING;
		Tools.PlaySoundFromGroup(sound_mag_insertion, kGunMechanicVolume);
		mag_seated = 0.0f;
	}
	
	void IsCylinderOpen() {
		return yolk_stage == YolkStage.OPEN || yolk_stage == YolkStage.OPENING;
	}
	
	bool AddRoundToCylinder() {
		if(gun_type != GunType.REVOLVER || yolk_stage != YolkStage.OPEN){
			return false;
		}
		var best_chamber = -1;
		var next_shot = active_cylinder;
		if(!IsHammerCocked()){
			next_shot = (next_shot + 1) % cylinder_capacity;
		}
		for(int i = 0; i<cylinder_capacity; ++i){
			var check = (next_shot + i)%cylinder_capacity;
			if(check < 0){
				check += cylinder_capacity;
			}
			if(!cylinders[check].object){
				best_chamber = check;
				break;
			}
		}
		if(best_chamber == -1){
			return false;
		}
		var yolk_pivot = transform.FindChild("yolk_pivot");
		if(yolk_pivot){
			var yolk = yolk_pivot.FindChild("yolk");
			if(yolk){
				var cylinder_assembly = yolk.FindChild("cylinder_assembly");
				if(cylinder_assembly){
					var extractor_rod = cylinder_assembly.FindChild("extractor_rod");
					if(extractor_rod){
						var name = "point_chamber_"+(best_chamber+1);
						cylinders[best_chamber].object = Instantiate(casing_with_bullet, extractor_rod.FindChild(name).position, extractor_rod.FindChild(name).rotation);
						cylinders[best_chamber].object.transform.localScale = Vector3(1.0f,1.0f,1.0f);
						cylinders[best_chamber].can_fire = true;
						cylinders[best_chamber].seated = Random.Range(0.0f, 1.0f);
						var renderers = cylinders[best_chamber].object.GetComponentsInChildren(Renderer);
						for(Renderer in renderers){
							renderer.castShadows  renderer= false; 
						}
						Tools.PlaySoundFromGroup(sound_bullet_eject, kGunMechanicVolume);
						return true;
					}
				}
			}
		}
		return false;
	}
	
	bool ShouldOpenCylinder() {
		int num_firable_bullets  = 0;
		for(int i = 0; i<cylinder_capacity; ++i){
			if(cylinders[i].can_fire){
				++num_firable_bullets;
			}
		}
		return num_firable_bullets != cylinder_capacity;
	}
	
	bool ShouldCloseCylinder() {
		int num_firable_bullets  = 0;
		for(int i = 0; i<cylinder_capacity; ++i){
			if(cylinders[i].can_fire){
				++num_firable_bullets;
			}
		}
		return num_firable_bullets == cylinder_capacity;
	}
	
	bool ShouldExtractCasings() {
		int num_fired_bullets  = 0;
		for(int i = 0; i<cylinder_capacity; ++i){
			if(cylinders[i].object && !cylinders[i].can_fire){
				++num_fired_bullets;
			}
		}
		return num_fired_bullets > 0;
	}
	
	bool ShouldInsertBullet() {
		int num_empty_chambers  = 0;
		for(int i = 0; i<cylinder_capacity; ++i){
			if(!cylinders[i].object){
				++num_empty_chambers;
			}
		}
		return num_empty_chambers > 0;
	}
	
	bool HasSlide() {
		return has_slide;
	}
	
	bool HasSafety() {
		return has_safety;
	}
	
	bool HasHammer() {
		return has_hammer;
	}
	
	bool HasAutoMod() {
		return has_auto_mod;
	}
	
	bool ShouldToggleAutoMod() {
		return auto_mod_stage == AutoModStage.ENABLED;
	}
	
	bool IsHammerCocked() {
		return hammer_cocked == 1.0f;
	}
	
	bool ShouldPullBackHammer() {
		return hammer_cocked != 1.0f && has_hammer && action_type == ActionType.SINGLE;
	}
	
	bool SwingOutCylinder() {
		if(gun_type == GunType.REVOLVER && (yolk_stage == YolkStage.CLOSED || yolk_stage == YolkStage.CLOSING)){
			yolk_stage = YolkStage.OPENING;
			return true;
		} else {
			return false;
		}
	}
	
	bool CloseCylinder() {
		if(gun_type == GunType.REVOLVER && (extractor_rod_stage == ExtractorRodStage.CLOSED && yolk_stage == YolkStage.OPEN || yolk_stage == YolkStage.OPENING)){
			yolk_stage = YolkStage.CLOSING;
			return true;
		} else {
			return false;
		}
	}
	
	bool ExtractorRod() {
		if(gun_type == GunType.REVOLVER && (yolk_stage == YolkStage.OPEN && extractor_rod_stage == ExtractorRodStage.CLOSED || extractor_rod_stage == ExtractorRodStage.CLOSING)){
			extractor_rod_stage = ExtractorRodStage.OPENING;
			if(extractor_rod_amount < 1.0f){
				extracted = false;
			}
			return true;
		} else {
			return false;
		}
	}
	
	void RotateCylinder(int how_many) {
		/*while(how_many != 0){
		if(how_many > 0){
			active_cylinder = (active_cylinder + 1)%cylinder_capacity;
			--how_many;
		}
		if(how_many < 0){
			active_cylinder = (cylinder_capacity + active_cylinder - 1)%cylinder_capacity;
			++how_many;
		}
	}*/
		target_cylinder_offset += how_many * (Mathf.Max(1,Mathf.Abs(target_cylinder_offset)));
		target_cylinder_offset = Mathf.Max(-12, Mathf.Min(12, target_cylinder_offset));
	}
	
	bool IsPressCheck() {
		if(gun_type != GunType.AUTOMATIC){
			return false;
		}
		return slide_amount <= kPressCheckPosition && 
			((slide_stage == SlideStage.PULLBACK && slide_lock) || (slide_stage == SlideStage.HOLD));
	}
	
	void Update () {
		if(gun_type == GunType.AUTOMATIC){
			if(magazine_instance_in_gun){
				var mag_pos = transform.FindChild("point_mag_inserted").position;
				var mag_rot = transform.rotation;
				var mag_seated_display = mag_seated;
				if(disable_springs){
					mag_seated_display = Mathf.Floor(mag_seated_display + 0.5f);
				}
				mag_pos += (transform.FindChild("point_mag_to_insert").position - 
				            transform.FindChild("point_mag_inserted").position) * 
					(1.0f - mag_seated_display);
				magazine_instance_in_gun.transform.position = mag_pos;
				magazine_instance_in_gun.transform.rotation = mag_rot;
			}
			
			if(mag_stage == MagStage.INSERTING){
				mag_seated += Time.deltaTime * 5.0f;
				if(mag_seated >= 1.0f){
					mag_seated = 1.0f;
					mag_stage = MagStage.IN;
					if(slide_amount > 0.7f){
						ChamberRoundFromMag();
					}
					recoil_transfer_y += Random.Range(-40.0f,40.0f);
					recoil_transfer_x += Random.Range(50.0f, 300.0f);
					rotation_transfer_x += Random.Range(-0.4f,0.4f);
					rotation_transfer_y += Random.Range(0.0f, 1.0f);
				}
			}
			if(mag_stage == MagStage.REMOVING){
				mag_seated -= Time.deltaTime * 5.0f;
				if(mag_seated <= 0.0f){
					mag_seated = 0.0f;
					ready_to_remove_mag = true;
					mag_stage = MagStage.OUT;
				}
			}
		}
		
		if(has_safety){
			if(safety == Safety.OFF){
				safety_off = Mathf.Min(1.0f, safety_off + Time.deltaTime * 10.0f);
			} else if(safety == Safety.ON){
				safety_off = Mathf.Max(0.0f, safety_off - Time.deltaTime * 10.0f);
			}
		}
		
		if(has_auto_mod){
			if(auto_mod_stage == AutoModStage.ENABLED){
				auto_mod_amount = Mathf.Min(1.0f, auto_mod_amount + Time.deltaTime * 10.0f);
			} else if(auto_mod_stage == AutoModStage.DISABLED){
				auto_mod_amount = Mathf.Max(0.0f, auto_mod_amount - Time.deltaTime * 10.0f);
			}
		}
		
		if(thumb_on_hammer == Thumb.SLOW_LOWERING){
			hammer_cocked -= Time.deltaTime * 10.0f;
			if(hammer_cocked <= 0.0f){
				hammer_cocked = 0.0f;
				thumb_on_hammer = Thumb.OFF_HAMMER;
				Tools.PlaySoundFromGroup(sound_hammer_decock, kGunMechanicVolume);
				//Tools.PlaySoundFromGroup(sound_mag_eject_button, kGunMechanicVolume);
			}
		}
		
		if(has_slide){
			if(slide_stage == SlideStage.PULLBACK || slide_stage == SlideStage.HOLD){
				if(slide_stage == SlideStage.PULLBACK){
					slide_amount += Time.deltaTime * 10.0f;
					if(slide_amount >= kSlideLockPosition && slide_lock){
						slide_amount = kSlideLockPosition;
						slide_stage = SlideStage.HOLD;
						Tools.PlaySoundFromGroup(sound_slide_back, kGunMechanicVolume);
					}
					if(slide_amount >= kPressCheckPosition && slide_lock){
						slide_amount = kPressCheckPosition;
						slide_stage = SlideStage.HOLD;
						slide_lock = false;
						Tools.PlaySoundFromGroup(sound_slide_back, kGunMechanicVolume);
					}
					if(slide_amount >= 1.0f){
						PullSlideBack();
						slide_amount = 1.0f;
						slide_stage = SlideStage.HOLD;
						Tools.PlaySoundFromGroup(sound_slide_back, kGunMechanicVolume);
					}
				}
			}	
			
			var slide_amount_display = slide_amount;
			if(disable_springs){
				slide_amount_display = Mathf.Floor(slide_amount_display + 0.5f);
				if(slide_amount == kPressCheckPosition){
					slide_amount_display = kPressCheckPosition;
				}
			}
			transform.FindChild("slide").localPosition = 
				slide_rel_pos + 
					(transform.FindChild("point_slide_end").localPosition - 
					 transform.FindChild("point_slide_start").localPosition) * slide_amount_display;
		}
		
		if(has_hammer){
			var hammer = GetHammer();
			var point_hammer_cocked = transform.FindChild("point_hammer_cocked");
			var hammer_cocked_display = hammer_cocked;
			if(disable_springs){
				hammer_cocked_display = Mathf.Floor(hammer_cocked_display + 0.5f);
			}
			hammer.localPosition = 
				Vector3.Lerp(hammer_rel_pos, point_hammer_cocked.localPosition, hammer_cocked_display);
			hammer.localRotation = 
				Quaternion.Slerp(hammer_rel_rot, point_hammer_cocked.localRotation, hammer_cocked_display);
		}
		
		if(has_safety){
			var safety_off_display = safety_off;
			if(disable_springs){
				safety_off_display = Mathf.Floor(safety_off_display + 0.5f);
			}
			transform.FindChild("safety").localPosition = 
				Vector3.Lerp(safety_rel_pos, transform.FindChild("point_safety_off").localPosition, safety_off_display);
			transform.FindChild("safety").localRotation = 
				Quaternion.Slerp(safety_rel_rot, transform.FindChild("point_safety_off").localRotation, safety_off_display);
		}
		
		if(has_auto_mod){
			var auto_mod_amount_display = auto_mod_amount;
			if(disable_springs){
				auto_mod_amount_display = Mathf.Floor(auto_mod_amount_display + 0.5f);
			}
			var slide = transform.FindChild("slide");
			slide.FindChild("auto mod toggle").localPosition = 
				Vector3.Lerp(auto_mod_rel_pos, slide.FindChild("point_auto_mod_enabled").localPosition, auto_mod_amount_display);
		}
		
		if(gun_type == GunType.AUTOMATIC){
			hammer_cocked = Mathf.Max(hammer_cocked, slide_amount);
			if(hammer_cocked != 1.0f && thumb_on_hammer == Thumb.OFF_HAMMER  && (pressure_on_trigger == PressureState.NONE || action_type == ActionType.SINGLE)){
				hammer_cocked = Mathf.Min(hammer_cocked, slide_amount);
			}
		} else {
			if(hammer_cocked != 1.0f && thumb_on_hammer == Thumb.OFF_HAMMER && (pressure_on_trigger == PressureState.NONE || action_type == ActionType.SINGLE)){
				hammer_cocked = 0.0f;
			}
		}
		
		if(has_slide){
			if(slide_stage == SlideStage.NOTHING){
				var old_slide_amount = slide_amount;
				slide_amount = Mathf.Max(0.0f, slide_amount - Time.deltaTime * kSlideLockSpeed);
				if(!slide_lock && slide_amount == 0.0f && old_slide_amount != 0.0f){
					Tools.PlaySoundFromGroup(sound_slide_front, kGunMechanicVolume*1.5f);
					if(round_in_chamber){
						round_in_chamber.transform.position = transform.FindChild("point_chambered_round").position;
						round_in_chamber.transform.rotation = transform.FindChild("point_chambered_round").rotation;
					}
				}
				if(slide_amount == 0.0f && round_in_chamber_state == RoundState.LOADING){
					round_in_chamber_state = RoundState.READY;
				}
				if(slide_lock && old_slide_amount >= kSlideLockPosition){
					slide_amount = Mathf.Max(kSlideLockPosition, slide_amount);
					if(old_slide_amount != kSlideLockPosition && slide_amount == kSlideLockPosition){
						Tools.PlaySoundFromGroup(sound_slide_front, kGunMechanicVolume);
					}
				}
			}
		}
		
		if(gun_type == GunType.REVOLVER){
			if(yolk_stage == YolkStage.CLOSED && hammer_cocked == 1.0f){
				target_cylinder_offset = 0;
			}
			if(target_cylinder_offset != 0.0f){
				var target_cylinder_rotation = ((active_cylinder + target_cylinder_offset) * 360.0f / cylinder_capacity);
				cylinder_rotation = Mathf.Lerp(target_cylinder_rotation, cylinder_rotation, Mathf.Pow(0.2f,Time.deltaTime));
				if(cylinder_rotation > (active_cylinder + 0.5f)  * 360.0f / cylinder_capacity){
					++active_cylinder;
					--target_cylinder_offset;
					if(yolk_stage == YolkStage.CLOSED){
						Tools.PlaySoundFromGroup(sound_cylinder_rotate, kGunMechanicVolume);
					}
				}
				if(cylinder_rotation < (active_cylinder - 0.5f)  * 360.0f / cylinder_capacity){
					--active_cylinder;
					++target_cylinder_offset;
					if(yolk_stage == YolkStage.CLOSED){
						Tools.PlaySoundFromGroup(sound_cylinder_rotate, kGunMechanicVolume);
					}
				}
			}
			if(yolk_stage == YolkStage.CLOSING){
				yolk_open -= Time.deltaTime * 5.0f;
				if(yolk_open <= 0.0f){
					yolk_open = 0.0f;
					yolk_stage = YolkStage.CLOSED;
					Tools.PlaySoundFromGroup(sound_cylinder_close, kGunMechanicVolume * 2.0f);
					target_cylinder_offset = 0;
				}
			}
			if(yolk_stage == YolkStage.OPENING){
				yolk_open += Time.deltaTime * 5.0f;
				if(yolk_open >= 1.0f){
					yolk_open = 1.0f;
					yolk_stage = YolkStage.OPEN;
					Tools.PlaySoundFromGroup(sound_cylinder_open, kGunMechanicVolume * 2.0f);
				}
			}
			if(extractor_rod_stage == ExtractorRodStage.CLOSING){
				extractor_rod_amount -= Time.deltaTime * 10.0f;
				if(extractor_rod_amount <= 0.0f){
					extractor_rod_amount = 0.0f;
					extractor_rod_stage = ExtractorRodStage.CLOSED;
					Tools.PlaySoundFromGroup(sound_extractor_rod_close, kGunMechanicVolume);
				}
				for(int i = 0; i<cylinder_capacity; ++i){
					if(cylinders[i].object){
						cylinders[i].falling = false;
					}
				}
			}
			if(extractor_rod_stage == ExtractorRodStage.OPENING){
				var old_extractor_rod_amount = extractor_rod_amount;
				extractor_rod_amount += Time.deltaTime * 10.0f;
				if(extractor_rod_amount >= 1.0f){
					if(!extracted){
						for(i=0; i<cylinder_capacity; ++i){
							if(cylinders[i].object){
								if(Random.Range(0.0f, 3.0f) > cylinders[i].seated){
									cylinders[i].falling = true;
									cylinders[i].seated -= Random.Range(0.0f, 0.5f);
								} else {
									cylinders[i].falling = false;
								}
							}
						}
						extracted = true;
					}
					for(i=0; i<cylinder_capacity; ++i){
						if(cylinders[i].object && cylinders[i].falling){
							cylinders[i].seated -= Time.deltaTime * 5.0f;
							if(cylinders[i].seated <= 0.0f){
								var bullet = cylinders[i].object;
								bullet.AddComponent(Rigidbody);
								bullet.transform.parent = null;
								bullet.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
								bullet.rigidbody.velocity = velocity;
								bullet.rigidbody.angularVelocity = Vector3(Random.Range(-40.0f,40.0f),Random.Range(-40.0f,40.0f),Random.Range(-40.0f,40.0f));
								cylinders[i].object = null;
								cylinders[i].can_fire = false;
							}
						}
					}
					extractor_rod_amount = 1.0f;
					extractor_rod_stage = ExtractorRodStage.OPEN;
					if(old_extractor_rod_amount < 1.0f){
						Tools.PlaySoundFromGroup(sound_extractor_rod_open, kGunMechanicVolume);
					}
				}
			}
			if(extractor_rod_stage == ExtractorRodStage.OPENING || extractor_rod_stage == ExtractorRodStage.OPEN){
				extractor_rod_stage = ExtractorRodStage.CLOSING;
			}
			
			var yolk_open_display = yolk_open;
			var extractor_rod_amount_display = extractor_rod_amount;
			if(disable_springs){
				yolk_open_display = Mathf.Floor(yolk_open_display + 0.5f);
				extractor_rod_amount_display = Mathf.Floor(extractor_rod_amount_display + 0.5f);
			}
			var yolk_pivot = transform.FindChild("yolk_pivot");
			yolk_pivot.localRotation = Quaternion.Slerp(yolk_pivot_rel_rot, 
			                                            transform.FindChild("point_yolk_pivot_open").localRotation,
			                                            yolk_open_display);
			var cylinder_assembly = yolk_pivot.FindChild("yolk").FindChild("cylinder_assembly");
			cylinder_assembly.localRotation.eulerAngles.z = cylinder_rotation;	
			var extractor_rod = cylinder_assembly.FindChild("extractor_rod");
			extractor_rod.localPosition = Vector3.Lerp(
				extractor_rod_rel_pos, 
				cylinder_assembly.FindChild("point_extractor_rod_extended").localPosition,
				extractor_rod_amount_display);	
			
			for(i=0; i<cylinder_capacity; ++i){
				if(cylinders[i].object){
					var name = "point_chamber_"+(i+1);
					var bullet_chamber = extractor_rod.FindChild(name);
					cylinders[i].object.transform.position = bullet_chamber.position;
					cylinders[i].object.transform.rotation = bullet_chamber.rotation;
					cylinders[i].object.transform.localScale = transform.localScale;
				}
			}
		}
	}
	
	void FixedUpdate() {
		velocity = (transform.position - old_pos) / Time.deltaTime;
		old_pos = transform.position;
	}
}

