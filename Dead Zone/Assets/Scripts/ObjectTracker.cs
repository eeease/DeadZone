using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTracker : MonoBehaviour {
    public Transform objToTrack;
    public float zOffset, yOffset, xOffset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //~~! this gets wonky with camera angle changes.
        if (!GameManager.GM.restarting)
            transform.position = new Vector3((objToTrack.position.x + xOffset), (objToTrack.position.y + yOffset), (objToTrack.position.z + zOffset));

    }
}
