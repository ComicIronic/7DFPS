using UnityEngine;
using System.Collections;

public class tapescript : MonoBehaviour
{
    private float life_time = 0.0f;
    private Vector3 old_pos;

    void Start()
    {
        old_pos = transform.position;
    }

    void Update()
    {
        transform.Find("light_obj").GetComponent<Light>().intensity = 1.0f + Mathf.Sin(Time.time * 2.0f);
    }

    void FixedUpdate()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        Collider collider = GetComponent<Collider>();
        if (rigidbody && !rigidbody.IsSleeping() && collider && collider.enabled)
        {
            life_time += Time.deltaTime;
            RaycastHit hit;
            if (Physics.Linecast(old_pos, transform.position, out hit, 1))
            {
                transform.position = hit.point;
                transform.GetComponent<Rigidbody>().velocity *= -0.3f;
            }
            if (life_time > 2.0f)
            {
                GetComponent<Rigidbody>().Sleep();
            }
        }
        old_pos = transform.position;
    }
}

