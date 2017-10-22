using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieTimer : MonoBehaviour {
    public float dieAfter = 4f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        dieAfter -= Time.deltaTime;
        if (dieAfter <= 0)
        {
            Destroy(gameObject);
        }
	}
}
