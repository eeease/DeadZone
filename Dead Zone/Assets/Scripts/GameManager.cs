using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager GM;

    [Header("Debug Stuff")]
    public bool keyboardControls;

    [Header("Game Options")]
    public bool sliders = true; //if true, the speed gauges above players will show
    public bool deathMatch = true; //if true, when you hit a wall/back of cam/pit, you're out
    public bool coinsOn = true; //if true, the game will count down and award coins every couple of seconds
    public bool stockMatch = false; //if true, the game will award a winner based on who has the most coins at the end of the round (or if only one player is left)
    public bool multiplayer = true; //if true, there will be a winner when there's only one rider left
    public bool clearWalls = true;
    public bool randomWidths = false; //if true, walls spawn with random z scales

    [Header("Coin Options")]
    public int coinTarget;//once a player has reached this, that player wins.
    public float coinCountdown, coinCountdownOG; //set how often the coins will spawn

    [Header("Game State Vars")]
    public bool winnerDeclared;

    [Header("Player Prefabs")]
    public GameObject player1;
    public GameObject player2;

    CamTrack cam;
    public List<GameObject> players;
    public PolePositionBehavior pole;
    public bool restarting = false;
    public List<Vector3> startPositions;

    void Awake()
    {
        if (GM == null)
        {
            DontDestroyOnLoad(this);
            GM = this;
        }else if (GM != this)
        {
            Destroy(gameObject);
        }
        
    }

	// Use this for initialization
	void Start () {
        cam = Camera.main.GetComponent<CamTrack>();
        pole = GameObject.Find("PolePositionCollider").GetComponent<PolePositionBehavior>();
        CalculatePosition();

        //check controller input right quick:
        foreach(string joystick in Input.GetJoystickNames())
        {
            if (joystick.ToLower().Contains("xbox") || joystick.ToLower().Contains("wireless"))
            {
                //set debug keyboard controls to be false:
                keyboardControls = false;
            }
        }

        foreach(GameObject tform in GameObject.FindGameObjectsWithTag("StartPos"))
        {
            startPositions.Add(tform.transform.position);
        }


    }

    // Update is called once per frame
    void Update () {
        if (winnerDeclared)
        {
            if (Input.GetButtonDown("Restart"))
            {
                QuickRestart();
            }
        }
        //Coins On Logic:
        if (coinsOn && !UIManager.UI.startGameCountdown && !winnerDeclared) //don't countdown at the beginning of a round.
        {
            coinCountdown -= Time.deltaTime;
            if (coinCountdown <= 0)
            {
                AwardCoins();
                coinCountdown = coinCountdownOG;
            }   
        }
		
	}

    public void CheckForWinner()
    {
    }
    //~~this function needs to be re-looked at.  right now it's doing too much.
    public void CalculatePosition()
    {
        players.Clear();
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (deathMatch)
            {
                if (!player.GetComponent<PlayerControl>().crashing && !player.GetComponent<PlayerControl>().imTheWinner) //don't add the crashing players to the list
                    players.Add(player); //add all players to a list
            }else if (stockMatch)
            {
                if (!player.GetComponent<PlayerControl>().imALoser && !player.GetComponent<PlayerControl>().imTheWinner) //don't add the crashing players to the list
                    players.Add(player); //add all players to a list

            }
        }

        //check if there's a deathmatch winner at this point:
        if (players.Count == 0)
        {
            DeclareDraw();
        }
        else if (players.Count == 1)
        {
            if (deathMatch) //if deathmatch is true, whoever's left is the winner
            {
                DeclareWinner(players[0]);
            }
            MoveTheChains();
        }
        else //do the following if there's more than one person left:
        {

            //sort them by their z pos (first place = 0th element)
            players.Sort((y, x) => x.transform.position.z.CompareTo(y.transform.position.z));

            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<PlayerControl>().currentPosition = i + 1;
            }

            MoveTheChains();
        }
    }

    /// <summary>
    /// this is a football joke but practically, it will move the pole position collider to in front of the first place player.
    /// </summary>
    public void MoveTheChains()
    {
        //if (deathMatch)
        //{
            pole.playerInFirst = players[0].transform;
            cam.playerInFirst = players[0].transform;
        //}else if (stockMatch)
        //{
        //    foreach(GameObject player in players)
        //    {
        //        if (player.GetComponent<PlayerControl>().imTheWinner)
        //        {
        //            pole.playerInFirst = player.transform;
        //            cam.playerInFirst = player.transform;

        //        }
        //    }
        //}

    }

    public void SwitchOffPlayerSliders()
    {
        if (sliders)
        {
            sliders = false;
        }else
        {
            sliders = true;
        }
        foreach(GameObject player in players)
        {
            player.GetComponent<PlayerControl>().SwitchSliders();
        }
    }
    public void SwitchRandWidths()
    {
        if (randomWidths)
        {
            randomWidths = false;
        }
        else
        {
            randomWidths = true;
        }
    }

    public void SwitchOffCoinAttack()
    {
        if (coinsOn)
        {
            coinsOn = false;
        }else
        {
            coinsOn = true;
        }
        UIManager.UI.TurnOffCoinUI();
    }
    public void SwitchClearWalls()
    {
        if (clearWalls)
        {
            clearWalls = false;
        }else
        {
            clearWalls = true;
        }
    }

    public void SwitchGameMode()
    {
        //grab the text component in ModeText:
        Text modeText = GameObject.Find("ModeText").GetComponent<Text>();
        if (deathMatch)
        {
            deathMatch = false;
            stockMatch = true;
            modeText.text = "Stock";
        }else
        {
            deathMatch = true;
            stockMatch = false;
            modeText.text = "Death Match";
        }
    }

    //public void SwitchStockMatch()
    //{
    //    //~~this turned out to be fairly hacky and probably inefficient.  the main problem was onValueChanged being called on the toggles, so had to circumvent that by disabling checkmark images.
    //    //get all the images in the toggle and turn off the checkmark (hopefully that doesn't trigger onValueChanged
    //    Image[] imgs = GameObject.Find("DeathMatchToggle").GetComponentsInChildren<Image>();
    //    Image[] stockimgs = GameObject.Find("StockMatchToggle").GetComponentsInChildren<Image>();

    //    if (stockMatch)
    //    {
    //        stockMatch = false;
    //        foreach (Image check in stockimgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = false;
    //            }
    //        }

    //        deathMatch = true;
    //        //this triggers OnValueChanged... ->GameObject.Find("DeathMatchToggle").GetComponent<Toggle>().isOn = true;
    //        foreach(Image check in imgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = true;
    //            }
    //        }

    //    }
    //    else
    //    {
    //        stockMatch = true;
    //        foreach (Image check in stockimgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = true;
    //            }
    //        }

    //        deathMatch = false;
    //        foreach (Image check in imgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = false;
    //            }
    //        }
    //    }
    //}

    //public void SwitchDM()
    //{
    //    Image[] imgs = GameObject.Find("StockMatchToggle").GetComponentsInChildren<Image>();
    //    Image[] dmimgs = GameObject.Find("DeathMatchToggle").GetComponentsInChildren<Image>();

    //    if (deathMatch)
    //    {
    //        deathMatch = false;
    //        foreach (Image check in dmimgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = false;
    //            }
    //        }

    //        stockMatch = true;

    //        foreach (Image check in imgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = true;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        deathMatch = true;
    //        foreach (Image check in dmimgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = true;
    //            }
    //        }

    //        stockMatch = false;
    //        foreach (Image check in imgs)
    //        {
    //            if (check.name == "Checkmark")
    //            {
    //                check.enabled = false;
    //            }
    //        }
    //    }
    //}


    public void AwardCoins()
    {
        foreach(GameObject player in players)
        {
            if(player.GetComponent<PlayerControl>().mySlider.value>0) //if they're actually moving forward (checking slider vs input might not be great but whatever, they're coupled
            player.GetComponent<PlayerControl>().AwardCoin();
            if (stockMatch) //if stock is enabled, do a check for the winner (~~ should this be done in player??)
            {
                if (player.GetComponent<PlayerControl>().earnedCoins >= coinTarget)
                {
                    DeclareWinner(player);
                }
            }
        }
    }
    
    public void DeclareWinner(GameObject playerWhoWins)
    {
        winnerDeclared = true;
        playerWhoWins.GetComponent<PlayerControl>().DisableMyStuff();

        playerWhoWins.GetComponent<PlayerControl>().imTheWinner = true;
        if (stockMatch) //in a stock match, destroy all the other players, cause f those guys.
        {
            foreach (GameObject player in players)
            {
                if (player != playerWhoWins)
                {
                    player.GetComponent<PlayerControl>().imALoser = true;
                }
            }
        }
        UIManager.UI.SwitchWinUI(true);
    }

    public void DeclareDraw()
    {
        winnerDeclared = true;
        cam.playerInFirst = pole.transform; //make it follow the pole?
        UIManager.UI.SwitchDrawUI(true); //~for now just switch on the winner text.  should be changed to draw.
    }

    public void QuickRestart()
    {
        //~~~a lot of this functionality is hacky and should be refactored when i get a better idea of feature scope:
        restarting = true; //so other stuff knows to stop behaving while this is restarting (pole position behavior, ex)

        //for(int i=0; i<players.Count; i++)
        //{
        //    Debug.Log("removing" + players[i]);

        //    players.Remove(players[i]);
        //}
        //Destroy(GameObject.FindGameObjectWithTag("Player"));
        //for (int i = players.Count-1; i>0; i--)
        //{
            //players[0].GetComponent<PlayerControl>().CrashAndDie(players[0].GetComponent<PlayerControl>().playerNum);
        //}
        ////count backwards through list and remove null items, cause lists are sometimes stupid about how they remove/destroy gos;
        players.RemoveAll(item => item == null);

        Instantiate(player1, startPositions[0], Quaternion.Euler(0,180,0));
        Instantiate(player2, startPositions[1], Quaternion.Euler(0, 180, 0));
        CalculatePosition();

        if (clearWalls)
        {
            DestroyWalls();
        }
        UIManager.UI.ResetGameCountdown();
        UIManager.UI.ResetSliderScores();
        UIManager.UI.SwitchWinUI(false);
        UIManager.UI.SwitchDrawUI(false);
        winnerDeclared = false;
        restarting = false;
    }

    public void DestroyWalls()
    {
        foreach(GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
        {
            if(wall.name!="BackOfCamCollider") //don't destroy the back of cam colliderrrr
            Destroy(wall);
        }
    }




}
