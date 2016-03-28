using UnityEngine;
using System.Collections;

public class LevelScript : MonoBehaviour
{
    void Start()
    {
        var enemies = transform.Find("enemies");
        if (enemies)
        {
            foreach (Transform child in enemies)
            {
                if (Random.Range(0.0f, 1.0f) < 0.9f)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        var players = transform.Find("player_spawn");
        if (players)
        {
            int num = 0;
            foreach (Transform child in players)
            {
                ++num;
            }
            var save = Random.Range(0, num);
            int j = 0;
            foreach (Transform child in players)
            {
                if (j != save)
                {
                    GameObject.Destroy(child.gameObject);
                }
                ++j;
            }
        }
        var items = transform.Find("items");
        if (items)
        {
            foreach (Transform child in items)
            {
                if (Random.Range(0.0f, 1.0f) < 0.9f)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
    }

    void Update()
    {

    }
}

