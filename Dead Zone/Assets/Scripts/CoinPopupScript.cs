using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPopupScript : MonoBehaviour {
    public float timeTillDeath = .8f;
    [Header("Rotation Selection")]
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;

    [Header("Translate Selection")]
    public bool slide;
    
    public float rotateSpeed = 200;
    public Vector3 slideDist;
    public float slideSpeed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        timeTillDeath -= Time.deltaTime;
        if (timeTillDeath >= .3f)
        {
            if (slide)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, slideDist, slideSpeed);
            }

            if (rotateX)
            {
                transform.Rotate(Vector3.right * Time.deltaTime * rotateSpeed);
            }
            if (rotateY)
            {
                transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed);

            }
            if (rotateZ)
            {
                transform.Rotate(Vector3.back * Time.deltaTime * rotateSpeed);
            }
        }else if (timeTillDeath <= 0)
        {
            Destroy(gameObject);
        }
	}
}
