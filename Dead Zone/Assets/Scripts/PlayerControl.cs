using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour {

    [Header("Movement Vars")]
    public float speedOG;
    public float speed;
    public float maxSpeed;
    public float speedBoostTimer;
    public float speedBoostTimerOG;
    public float pauseTimerOG, pauseTimer;
    public bool pauseForWinner;
    public Vector3 defaultSpeed = new Vector3(0, 0, .5f);
    public Vector3 slowMoSpeed = new Vector3(0, 0, .1f);
    public AudioClip cruisinLoop;
    public float pitchDiff; //how much does the cruise sfx jump up each speed boost

    Rigidbody rb;
    public GameObject explosionPrefab; //this will be loaded with a real RAD explosion that will drop on crash
    [Header("Race Standings Info")]
    public int playerNum;
    public int currentPosition;
    public float spinOutTimer, spinOutTimerOG, spinSpeed;
    public bool spinningOut = false;
    public bool crashing = false;
    public int earnedCoins;
    public bool imTheWinner;
    public bool imALoser;

    [Header("Floaty Movement Vars")]
    public float period;
    public float rangeY;
    private Vector3 originalPos;

    [Header("HUD Vars")]
    public Canvas myCan;
    public Slider mySlider;
    public GameObject coinPrefab;
    public GameObject crazyCoin;

    //stuff that doesn't need to be exposed:
    float horInput, vertInput;
    Vector3 velocity;
    Quaternion originalRot;
    CamTrack cam;
    bool ghosting = false;
    bool closeCallCoin = false;
    BoxCollider bCollider;
    float tempSpeed;
    GameObject backOfCamCollider; //going to need to track this in order to detect 'collision' during ghosting
    AudioSource aSource;
    float pitchOG;

    // Use this for initialization
    void Awake() {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.GetComponent<CamTrack>();
        originalRot = transform.rotation;
        bCollider = GetComponent<BoxCollider>();
        myCan = GetComponentInChildren<Canvas>();
        mySlider = myCan.GetComponentInChildren<Slider>();
	}

    void Start()
    {
        originalPos = transform.localPosition;
        pauseTimer = pauseTimerOG;
        backOfCamCollider = GameObject.Find("BackOfCamCollider");
        crashing = false;
        aSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        //check if you're the winner before any other logic:
        if (imTheWinner || imALoser)
        {
            EndGameStuff();
        }
      //ghosting support:
            if (Input.GetButtonDown("GhostRideTheWhip" + playerNum))
            {
                if (closeCallCoin)
                {
                    AwardCrazyCoin();
                    closeCallCoin = false;
                }
                Ghosting();
            }
            if (Input.GetButtonUp("GhostRideTheWhip" + playerNum))
            {
                GetComponent<MeshRenderer>().enabled = true;

                if (!imTheWinner) //don't turn the collider back on if you're doing your victory lap
                    bCollider.enabled = true;


                ghosting = false;
                speed = tempSpeed;
            }

        if (ghosting)
        {

            //while ghosting, check to see if you're off the back of the screen
            if (transform.position.z < backOfCamCollider.transform.position.z)
            {
                crashing = true;
            }
            //while you're ghosting, check to see if you're taking the pole position
            if(transform.position.z > GameManager.GM.pole.transform.position.z)
            {
                GameManager.GM.CalculatePosition(); 
            }
        }

            //don't do any of the following movement logic if the game is counting down to start
        if (!UIManager.UI.startGameCountdown)
        {
            velocity.z = Input.GetAxisRaw("Horizontal" + playerNum) * speed;
            //velocity.x = Input.GetAxisRaw("Vertical" + playerNum)*speed;
            if (!pauseForWinner)
            {
                if (!spinningOut)
                {
                    if (!crashing  && !imALoser)
                    {
                        SliderUpdate(); //only update the slider if you're not spinning out.  else disable it (so it doesn't spin around with you)

                        if ((Input.GetAxisRaw("Horizontal" + playerNum) < 1) || GameManager.GM.keyboardControls) //~~should this also account for being >= 0?
                        {
                            if (Input.GetAxisRaw("Horizontal" + playerNum) > 0 && !imTheWinner && !ghosting) //if you're going more than 0, boost speed after a couple of seconds
                            {
                           
                                SpeedBoost();//run the speed boost logic
                            }
                            else if (Input.GetAxisRaw("Horizontal" + playerNum) == 0 || imTheWinner) //if you're the winner or you're not inputting anything, float by at the default speed:
                            {
                                transform.position += (defaultSpeed * Time.deltaTime) + FloatLogic();
                            }
                            transform.position += (velocity * Time.deltaTime) + FloatLogic();
                            //rb.MovePosition(rb.position + velocity * Time.deltaTime);

                        }
                        else if (Input.GetAxisRaw("Horizontal" + playerNum) == 1 && !GameManager.GM.keyboardControls && !imTheWinner)
                        {
                            spinningOut = true;
                        }
                    }
                    else
                    {
                        CrashAndDie(playerNum);
                    }
                }
                else
                {
                    SpinOut();
                }
            }
            else
            {
                //if you're pausing for the winner, just stay put for a couple of seconds:
                transform.position += ((slowMoSpeed * Time.deltaTime) + FloatLogic());
                pauseTimer -= Time.deltaTime;
                if (pauseTimer <= 0)
                {
                    pauseForWinner = false;
                    pauseTimer = pauseTimerOG;

                }
            }

            //floaty podracer movement:
            //FloatLogic();
        }else
        {
            //~~~if it is start of game countdown, here's a hacky way to stop the crashing bug from happening:
            if (crashing)
            {
                crashing = false;
            }
        }
    }



    void OnTriggerEnter(Collider col)
    {
        //if this isn't in first place, then do the check
        if ((gameObject != GameManager.GM.players[0]) && col.tag == "PolePosition") //if you run into a trigger that's tagged player (the only trigger on the player is the capsule collider...atm)
        {
            Debug.Log("Pole Position Trigger Hit!");
            GameManager.GM.CalculatePosition(); //go ahead and recalc first place
        }

        //grant a bonus coin if you ghosted close to a wall (well, switch a bool that'll do that...)
        if (col.tag == "CloseCall")
        {
            closeCallCoin = true;
        }
        if (col.tag == "Wall") //trigger version of this is for the backOfCamCollider (dying cause you fell off the back of the screen.
        {
            if(transform.position.z > col.transform.position.z)
            crashing = true;
        }

    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Wall")
        {
            crashing = true;
        }
    }

    void SliderUpdate()
    {
        mySlider.value = Input.GetAxisRaw("Horizontal" + playerNum);
    }

    public void SpeedBoost()
    {
        speedBoostTimer -= Time.deltaTime;
        if (speedBoostTimer <= 0)
        {
            if (speed < maxSpeed)
            {
                speed++;
                aSource.pitch += pitchDiff;
            }
            speedBoostTimer = speedBoostTimerOG;
        }
    }

    public void Ghosting()
    {
        tempSpeed = speed; //grab the current speed;
        speed = defaultSpeed.z; //while ghosting, go slowly.
        if (bCollider.enabled)
        {
            //for now actually make it invisible.  ~~~ in the future, make it transparent
            GetComponent<MeshRenderer>().enabled = false;
            bCollider.enabled = false;
            ghosting = true;
        }
        

    }
    public void SpinOut()
    {
        mySlider.gameObject.SetActive(false);
        spinOutTimer -= Time.deltaTime;
        if (spinOutTimer > 0)
        {
            velocity = Vector3.zero;
            transform.Rotate(Vector3.up * spinOutTimer * spinSpeed);
        }else if (spinOutTimer <= 0)
        {
            spinOutTimer = spinOutTimerOG;
            transform.rotation = originalRot;
            speed = speedOG;
            spinningOut = false;
            mySlider.gameObject.SetActive(true);
        }
    }

    public Vector3 FloatLogic()
    {
        Vector3 floatOffset = new Vector3(0, (Mathf.Sin(Time.time * period) * rangeY), 0);
        //floatOffset.y = Mathf.Sin(Time.time * period) * rangeY;
        return floatOffset;
    }

    public void SwitchSliders()
    {
        if (GameManager.GM.sliders)
        {
            mySlider.gameObject.SetActive(true);

        }
        else
        {
            mySlider.gameObject.SetActive(false);
        }
    }

    public void AwardCoin()
    {
        if (!spinningOut)
        {
            if (!ghosting) //they shouldn't get coins while ghosting, right?
            {
                //dividing maxValue by 3 to cut it into thirds and also to not use a set value, in case i want to change the way i'm working this later.
                if (mySlider.value > 0 && mySlider.value < (mySlider.maxValue / 3)) //if you're in the first third of the slider, earn one coin
                {
                    earnedCoins++;
                }
                if (mySlider.value >= (mySlider.maxValue / 3) && mySlider.value < ((mySlider.maxValue / 3) * 2))
                {
                    earnedCoins += 2;
                }
                if (mySlider.value >= ((mySlider.maxValue / 3) * 2))
                {
                    earnedCoins += 3;
                }
                Instantiate(coinPrefab, transform);
            }
        }
        else
        {
            earnedCoins -= 3; //lose coins if you're spinning out when the coin timer strikes zero;
        }

        UIManager.UI.UpdateSliderScores(playerNum, earnedCoins);
    }
    public void AwardCrazyCoin()
    {
        if (!spinningOut)
        {
            earnedCoins++;
            Instantiate(crazyCoin, transform);

            UIManager.UI.UpdateSliderScores(playerNum, earnedCoins);
            Debug.Log("Crazy Money");
        }
    }

    public void CrashAndDie(int playerNumber)
    {
        //remove me from the list of current players:
        GameManager.GM.players.Remove(gameObject);
        if (currentPosition == 1) //if in first place, spawn particles shooting backwards
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.Euler(-135, 0, 0));
        }else
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.Euler(-45, 0, 0));

        }
        GameManager.GM.CalculatePosition();

        Destroy(gameObject);
    }

    public void EndGameStuff()
    {
        if (ghosting)
        {
            GetComponent<MeshRenderer>().enabled = true;
            ghosting = false;
            speed = slowMoSpeed.z;

        }
        //kick the player out of ghost mode to see the crash in slowmo:


        if (Input.GetButtonDown("Restart"))
        {
            Destroy(gameObject);
        }
    }

    public void DisableMyStuff() //to be called by GM immediately upon declaring a winner
    {
        pauseForWinner = true; //jamming this in here so it only happens once.
        aSource.pitch = 1; //reset the pitch of the cruisin sfx
        bCollider.enabled = false;

    }
}
