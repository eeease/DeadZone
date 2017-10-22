using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolePositionBehavior : MonoBehaviour {
    public Transform playerInFirst;
    public float zOffset; //how far in front of p1 should this be?

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(!GameManager.GM.restarting && playerInFirst!=null)
        transform.position = new Vector3(0, -4, (playerInFirst.position.z + zOffset));
	}
}
