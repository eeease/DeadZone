using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTrack : MonoBehaviour {
    public Transform playerInFirst, playerInLast;
    public Vector3 offset, camPos;
    public float camSpeed, windowDist, easeSpeed;

    [Header("Cam Rotations")]
    public Vector3 topDownRot;
    public Vector3 threeQuartersRot;
    public Vector3 sideScrollRot;
    public Vector3 shmupAngleRot;

    [Header("Cam Positions")]
    public Vector3 topDownPos;
    public Vector3 threeQuartersPos;
    public Vector3 sideScrollPos;
    public Vector3 shmupAnglePos;
    public float camFOVWinner = 120;

    [Header("Cam Bools")]
    public bool topDown;
    public bool threeQuarters;
    public bool sideScroll;
    public bool shmupCam;

    public int camAngleIndex;
    public int camAngleIndexMax = 3;

    Camera myCam;

    // Use this for initialization
    void Start () {
        myCam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        camPos = transform.position;

        //debug cam switching: //~~ note this gets a little wonky with backofcamcollider's current tracking behavior.
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if (CheckCamBools() && !GameManager.GM.winnerDeclared)
            {
                if (camAngleIndex < camAngleIndexMax)
                {
                    camAngleIndex++;
                }
                else
                {
                    camAngleIndex = 0;
                }
                SwitchCameraAngle(camAngleIndex);
            }
        }

        if (GameManager.GM.winnerDeclared)
        {
            myCam.fieldOfView = Mathf.MoveTowards(myCam.fieldOfView, camFOVWinner, easeSpeed * Time.deltaTime);
        }else
        {
            if (myCam.fieldOfView != 60)
            {
                myCam.fieldOfView = 60;
            }
        }

        //don't track during the restarting chunk of logic cause during that time, there is no playerInFirst
        if (!GameManager.GM.restarting)
        {
            if (Vector3.Distance(camPos, playerInFirst.position) > windowDist)
            {
                camPos = Vector3.Lerp(camPos, new Vector3(camPos.x, camPos.y, playerInFirst.position.z), camSpeed * Time.deltaTime); //Camera should go to the player's position excepting the distance from the track
                transform.position = camPos;
            }
        }



        //~~this seems redundant but not thinking of a new way to set it up right now.
        if (topDown)
        {
            topDownPos.z = playerInFirst.position.z;
            StartCoroutine(EaseToNewPos("TopDown"));
            //EaseToNewPos("TopDown");
        }
        if (threeQuarters)
        {
            threeQuartersPos.z = playerInFirst.position.z;
            StartCoroutine(EaseToNewPos("ThreeQuarters"));

            //EaseToNewPos("ThreeQuarters");
        }
        if (shmupCam)
        {
            shmupAnglePos.z = playerInFirst.position.z;
            StartCoroutine(EaseToNewPos("Shmup"));

            //EaseToNewPos("Shmup");
        }
        if (sideScroll)
        {
            sideScrollPos.z = playerInFirst.position.z;
            StartCoroutine(EaseToNewPos("SideScroll"));

            //EaseToNewPos("SideScroll");
        }

	}
    IEnumerator EaseToNewPos(string whatToSwitchTo)
    {
        
            Vector3 targetPos = Vector3.zero;//temp pos.
            Quaternion targetRot = Quaternion.Euler(Vector3.zero);
            switch (whatToSwitchTo)
            {
                case "TopDown":
                    targetPos = topDownPos;
                    targetRot = Quaternion.Euler(topDownRot);
                    break;

                case "ThreeQuarters":
                    targetPos = threeQuartersPos;
                    targetRot = Quaternion.Euler(threeQuartersRot);


                    break;

                case "SideScroll":
                    targetPos = sideScrollPos;
                    targetRot = Quaternion.Euler(sideScrollRot);



                    break;

                case "Shmup":
                    targetPos = shmupAnglePos;
                    targetRot = Quaternion.Euler(shmupAngleRot);

                    //transform.SetPositionAndRotation(shmupAnglePos, Quaternion.Euler(shmupAngleRot));

                    break;
            }
            //now go to the targetPos
            transform.position = Vector3.Lerp(transform.position, targetPos, easeSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, easeSpeed * Time.deltaTime);

            //wait until you're super close to the target pos

            if (Vector3.Distance(transform.position, targetPos) < .1f && transform.rotation == targetRot)
            {
                SwitchOffBools(); //then reset everything
            }
            yield return null;
        
    }

    //IEnumerator EaseToNewPos(string whatToSwitchTo)
    //{
    //    float waitSecs = 0;
    //    if (easeSpeed < 5)
    //    {
    //        waitSecs = 2;
    //    }else
    //    {
    //        waitSecs = 1;
    //    }
    //    switch (whatToSwitchTo)
    //    {
    //        case "TopDown":
    //            transform.position = Vector3.Lerp(transform.position, topDownPos, easeSpeed*Time.deltaTime);
    //            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(topDownRot), easeSpeed * Time.deltaTime);
    //            yield return new WaitForSeconds(waitSecs); //wait before turning off this bool
    //            break;

    //        case "ThreeQuarters":
    //            transform.position = Vector3.Lerp(transform.position, threeQuartersPos, easeSpeed * Time.deltaTime);
    //            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(threeQuartersRot), easeSpeed * Time.deltaTime);

    //            yield return new WaitForSeconds(waitSecs); //wait before turning off this bool

    //            break;

    //        case "SideScroll":
    //            transform.position = Vector3.Lerp(transform.position, sideScrollPos, easeSpeed * Time.deltaTime);
    //            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(sideScrollRot), easeSpeed * Time.deltaTime);


    //            yield return new WaitForSeconds(waitSecs); //wait before turning off this bool

    //            break;

    //        case "Shmup":
    //            transform.position = Vector3.Lerp(transform.position, shmupAnglePos, easeSpeed * Time.deltaTime);
    //            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(shmupAngleRot), easeSpeed * Time.deltaTime);

    //            //transform.SetPositionAndRotation(shmupAnglePos, Quaternion.Euler(shmupAngleRot));
    //            yield return new WaitForSeconds(waitSecs); //wait before turning off this bool

    //            break;
    //    }
    //    SwitchOffBools();
    //}

    /*
    public void EaseToNewPos(string whatToSwitchTo)
    {
        switch (whatToSwitchTo)
        {
            case "TopDown":
                camPos = Vector3.Lerp(transform.position, topDownPos, easeSpeed);

                //transform.SetPositionAndRotation(topDownPos, Quaternion.Euler(topDownRot));
                yield return new WaitForSeconds(easeSpeed);
                break;

            case "ThreeQuarters":
                transform.SetPositionAndRotation(threeQuartersPos, Quaternion.Euler(threeQuartersRot));
                
                break;

            case "SideScroll":
                transform.SetPositionAndRotation(sideScrollPos, Quaternion.Euler(sideScrollRot));

                break;

            case "Shmup":
                transform.SetPositionAndRotation(shmupAnglePos, Quaternion.Euler(shmupAngleRot));

                break;
        }

        SwitchOffBools();
    }
    */


    public void SwitchOffBools()
    {
        topDown = false;
        sideScroll = false;
        shmupCam = false;
        threeQuarters = false;
    }


    //this will only come back true if all bools are true
    public bool CheckCamBools()
    {
        bool allFalse = false;
        if (!topDown && !sideScroll && !shmupCam && !threeQuarters)
        {
            allFalse = true;
        }
        return allFalse;
    }

    public void SwitchCameraAngle(int whatAngle)
    {
        switch (whatAngle)
        {
            case 0:
                topDown = true;
                break;

            case 1:
                threeQuarters = true;
                break;

            case 2:
                sideScroll = true;
                break;

            case 3:
                shmupCam = true;
                break;
        }
    }

    }

