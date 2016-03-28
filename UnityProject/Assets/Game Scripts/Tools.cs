//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0f.30319.18444f
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Tools
{
	public static void PlaySoundFromGroup(this MonoBehaviour script, AudioClip[] group, float volume) {
		if (group.Length == 0) {
			return;
		}

        int which_shot = Random.Range(0, group.Length);
		script.gameObject.GetComponent<AudioSource>().PlayOneShot (group [which_shot], volume * PlayerPrefs.GetFloat ("sound_volume", 1.0f));
	}

	
	public static MonoBehaviour RecursiveHasScript(GameObject gobject, string script, int depth) {
		if (gobject.GetComponent (script)) {
			return  (MonoBehaviour)gobject.GetComponent (script);
		} else if (depth > 0 && gobject.transform.parent) {
			return RecursiveHasScript (gobject.transform.parent.gameObject, script, depth - 1);
		} else {
			return null;
		}
	}
	
	public static Quaternion RandomOrientation() {
		return Quaternion.EulerAngles(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360));
	}

	public static Vector3 mix( Vector3 a, Vector3 b, float  val) {
		return a + (b-a) * val;
	}
	
	public static Quaternion mix( Quaternion a, Quaternion b, float  val) {
		float angle  = 0.0f;
		var axis = new Vector3();
		(Quaternion.Inverse(b)*a).ToAngleAxis(out angle, out axis);
		if(angle > 180){
			angle -= 360;
		}
		if(angle < -180){
			angle += 360;
		}
		return a * Quaternion.AngleAxis(angle * -val, axis);
	}

	public static T Pop<T>(this List<T> list) {
		T element = list [0];
		list.RemoveAt (0);
		return element;
	}

    public static Transform FindDeepChild(this Transform parent, string name)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
            {
                return child;
            }
            else
            {
                Transform found = child.FindDeepChild(name);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

	public static Vector3 SetDimension(Vector3 vector, char dimension, float value) {
		switch(dimension) {
			case 'x' : return new Vector3(dimension, vector.y, vector.z);
			case 'y' : return new Vector3(vector.x, dimension, vector.z);
			case 'z' : return new Vector3(vector.x, vector.y, dimension);
		}
		throw new Exception("Only x, y, and z are valid");
	}
}

