using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDestroySelf : MonoBehaviour
{
    public float timer = 2.05f;
    
    void Update()
    {
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}
