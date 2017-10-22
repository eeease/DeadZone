using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour {
    public static TrackManager TM;
    public bool timeBasedModifiers, collisionBasedModifiers;
    public float timeToSpawn, timeToSpawnOG;

    [Header("Track Modifier Bools")]
    public bool ghostWalls; //spawn walls that players have to hold a button to ghost through
    public bool rhythmSection; //spawn a section that players have to input buttons during
    public bool jumpSection; //spawn pits/lava/whatever, who cares, nothing matters, that players have to jump over

    [Header("Track Zones")]
    public GameObject ghostWallToSpawn; //this should spawn V3(0,-4,playerInFirst+ distance
    public GameObject lavaToSpawn;
    public GameObject[] ghostWallsToSpawn;

    [Header("Zone Warnings")]
    public GameObject ghostWarning;
    public GameObject rhythmWarning;
    public GameObject jumpWarning;

    public float distanceToSpawnZone; //maybe randomize this?
    bool dropWarnings;
    public float warningInterval, warningIntervalOG;
    int warningIndex; //when this hits a certain number, switch from dropping warnings to dropping a zone
    public int warningIndexMax = 3;
    void Awake()
    {
        if (TM == null)
        {
            DontDestroyOnLoad(this);
            TM = this;
        }
        else if (TM != this)
        {
            Destroy(gameObject);
        }

    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (timeBasedModifiers)
        {
            timeToSpawn -= Time.deltaTime;
            if (timeToSpawn <= 0)
            {
                //dropWarnings = true;
                SpawnZone(0);

                timeToSpawn = timeToSpawnOG;
            }
        }

        //i initially separated warnings so that they could be dynamic based on player speed (if players are going faster, they'd be more spread apart).  moving away from this to ensure equal distance between warnings (childed to wall).
        //if (dropWarnings)
        //{
        //    warningInterval -= Time.deltaTime;
        //    if (warningInterval <= 0)
        //    {
        //        if (warningIndex < 3)
        //        {
        //            DropWarning(0);
        //            warningIndex++;
        //            warningInterval = warningIntervalOG;
        //        }else
        //        {
        //            SpawnZone(0);
        //            warningIndex = 0;
        //            warningInterval = warningIntervalOG;
        //            dropWarnings = false;
        //        }
        //    }
        //}
		
	}
    /// <summary>
    /// 0 = ghost wall; 1 = jump section; 2 = rhythm section
    /// </summary>
    /// <param name="warningType"></param>
    public void DropWarning(int warningType)
    {
        Vector3 spawnDist = new Vector3(0, -4.1f, GameManager.GM.pole.gameObject.transform.position.z + distanceToSpawnZone);
        switch (warningType)
        {
            case 0:
                Instantiate(ghostWarning, spawnDist, Quaternion.Euler(90,0,0));
                break;


        }
    }

    public void SpawnZone(int zoneType)
    {
        Vector3 spawnDist = new Vector3(0, -4.1f, GameManager.GM.pole.gameObject.transform.position.z + distanceToSpawnZone);

        switch (zoneType)
        {
            case 0:
                if (GameManager.GM.randomWidths)
                {
                    
                    Instantiate(ghostWallsToSpawn[Random.Range(0, ghostWallsToSpawn.Length)], spawnDist, Quaternion.identity);

                }
                else
                {
                    Instantiate(ghostWallsToSpawn[0], spawnDist, Quaternion.identity);
                }

                break;
        }
    }

    public void DestroyAllObstacles()
    {

    }
}
