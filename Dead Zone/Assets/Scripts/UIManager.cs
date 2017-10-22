using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class UIManager : MonoBehaviour {
    public static UIManager UI;

    public bool turnOffDebugUI = false;

    public Canvas overlayCan;
    public List<Slider> playerSliders;
    public List<Text> sliderScores;

    [Header("Start Game Vars")]
    public bool startGameCountdown = true;
    public float countdown;
    public float countdownOG = 3f;
    public float pitchChange = .5f;

    public AudioClip[] uiClips;

    AudioSource aSource;
    Text coinTimer, coinText, winnerText, countDownText, drawText;
    int tempInt; //for counting down.


    void Awake()
    {
        if (UI == null)
        {
            DontDestroyOnLoad(this);
            UI = this;
        }
        else if (UI != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start () {
        overlayCan = GameObject.Find("OverlayCanvas").GetComponent<Canvas>();
        //Assign all important UI elements in overlayCan:
        foreach(Text txt in overlayCan.GetComponentsInChildren<Text>())
        {
            switch (txt.name)
            {
                case "CoinTimer":
                    coinTimer = txt;
                    break;
                case "CoinText":
                    coinText = txt;
                    break;
                case "WinnerText":
                    winnerText = txt;
                    break;
                case "CountDownText":
                    countDownText = txt;
                    break;
                case "DrawText":
                    drawText = txt;
                    break;
            }
            aSource = GetComponent<AudioSource>();
        }

        //a for loop is better suited for getting all the player stuff (Cause of numbers, ya know?):
        for(int i = 0; i< overlayCan.GetComponentsInChildren<Slider>().Length; i++)
        {
            playerSliders.Add(overlayCan.GetComponentsInChildren<Slider>()[i]); //
            playerSliders[i].maxValue = GameManager.GM.coinTarget;
            playerSliders[i].value = 0; //start them off with fresh sliders.

            sliderScores.Add(playerSliders[i].GetComponentInChildren<Text>());
            sliderScores[i].text = "0/" + GameManager.GM.coinTarget;
        }
        //for (int i = 0; i < overlayCan.GetComponentsInChildren<Text>().Length; i++)
        //{
        //    if (overlayCan.GetComponentsInChildren<Text>()[i].name.Contains("ScoreText")) //only add it to the list if it's a scoretext obj
        //    {
        //        sliderScores.Add(overlayCan.GetComponentsInChildren<Text>()[i]); //
        //        sliderScores[i].text = "0/" + GameManager.GM.coinTarget;
        //    }
        //}

        //sort the lists so the 0th element is p1 and so on.
        playerSliders.Sort((x, y) => x.name.CompareTo(y.name));
        sliderScores.Sort((x, y) => x.name.CompareTo(y.name));

        if (turnOffDebugUI) TurnOffDebugUI();
        winnerText.gameObject.SetActive(false);
        drawText.gameObject.SetActive(false);
        countdown = countdownOG;

    }

    // Update is called once per frame
    void Update () {

        if (startGameCountdown)
        {
            countdown -= Time.deltaTime;
            countDownText.text = countdown.ToString("0.00");
            if((int)countdown != tempInt) //if it is not what it was... play some sound.
            {
                aSource.Play();
                tempInt = (int)countdown;
            }
            if (countdown <= 0)
            {
                aSource.pitch += pitchChange;
                aSource.Play();
                //disable the text obj and stop counting down.
                countDownText.gameObject.SetActive(false);
                startGameCountdown = false;
            }
        }
        coinTimer.text = GameManager.GM.coinCountdown.ToString("0.00");

    }


    public void TurnOffDebugUI()
    {
        //off with the toggles:
        foreach(Toggle tog in overlayCan.GetComponentsInChildren<Toggle>())
        {
            tog.gameObject.SetActive(false);
        }
    }

    public void TurnOffCoinUI()
    {

        if (!GameManager.GM.coinsOn)
        {
            foreach (Slider slider in playerSliders)
            {
                slider.gameObject.SetActive(false);
            }
            foreach (Text score in sliderScores)
            {
                score.gameObject.SetActive(false);
            }
            coinText.enabled = false;
            coinTimer.enabled = false;
        }
        else
        {
            foreach (Slider slider in playerSliders)
            {
                slider.gameObject.SetActive(true);
            }
            foreach (Text score in sliderScores)
            {
                score.gameObject.SetActive(true);
            }
            coinText.enabled = true;
            coinTimer.enabled = true;
        }

        
    }

    /// <summary>
    /// to be called by players when they get coins:
    /// </summary>
    /// <param name="playerNum"></param>
    public void UpdateSliderScores(int playerNum, int currentCoins)
    {
        sliderScores[playerNum - 1].text = currentCoins + "/" + GameManager.GM.coinTarget;
        playerSliders[playerNum - 1].value = currentCoins;
    }

    public void ResetSliderScores()
    {
        foreach(Text slid in sliderScores)
        {
            slid.text = "0/" + GameManager.GM.coinTarget;
        }
        foreach(Slider slid in playerSliders)
        {
            slid.value = 0;
        }
    }

    public void SwitchWinUI(bool onOrOff)
    {
        if (onOrOff)
        {
            winnerText.gameObject.SetActive(true);
        }else
        {
            winnerText.gameObject.SetActive(false);
        }
    }
    public void SwitchDrawUI(bool onOrOff)
    {
        if (onOrOff)
        {
            drawText.gameObject.SetActive(true);
        }
        else
        {
            drawText.gameObject.SetActive(false);
        }
    }
    public void ResetGameCountdown()
    {
        countdown = countdownOG;
        countDownText.gameObject.SetActive(true);
        startGameCountdown = true;
        tempInt = (int)countdown; //store what the int was

        ClipAssign(0); //assign the start bell sfx to the source
        aSource.Play();
    }

    public void ClipAssign(int audioClipIndex)
    {
        aSource.clip = uiClips[audioClipIndex];
        if (aSource.pitch != 1)
        {
            aSource.pitch = 1;
        }

    }
}
