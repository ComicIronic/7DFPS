using UnityEngine;
using System.Collections;

public class plasterpuffscript : taser_spark
{
    void Update()
    {
        UpdateColor();
        opac -= Time.deltaTime * 10.0f;
        transform.localScale += new Vector3(1, 1, 1) * Time.deltaTime * 30.0f;
        if (opac <= 0.0f)
        {
            Destroy(gameObject);
        }
    }
}

