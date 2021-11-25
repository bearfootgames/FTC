using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotGameManager : MonoBehaviour
{
    [SerializeField] GameObject robot;
    [SerializeField] GameObject[] objectsToSpawnInWarehouse;
    [SerializeField] int[] numObjectsToSpawn;

    [SerializeField] GameObject duck;
    [SerializeField] Transform[] blueTeamDuckSpawns, redTeamDuckSpawns;
    [SerializeField] Transform blueSpawn, redSpawn;
    [SerializeField] Transform blueItemSpawn, redItemSpawn;
    [SerializeField] Transform redCarouselDuckSpawn, blueCarouselDuckSpawn;

    List<GameObject> spawnedItems;

    [SerializeField] float itemOffset;

    GameTimer timer;

    public ScoreKeeper scoreKeeper;

    bool gameStarted;

    void Start()
    {
        spawnedItems = new List<GameObject>();
        timer = FindObjectOfType<GameTimer>();
       // SpawnRobots();
    }

    private void Awake()
    {
        if (!scoreKeeper)
        {
            scoreKeeper = GameObject.Find("ScoreKeeper").GetComponent<ScoreKeeper>();
        }
    }

    void SpawnRobots()
    {
        Instantiate(robot, blueSpawn.position, Quaternion.identity);
    }

    void SpawnDucks()
    {
        int spawnIndex = Random.Range(0, 3);

        SpawnItem(duck, blueTeamDuckSpawns[spawnIndex].position);
        SpawnItem(duck, redCarouselDuckSpawn.position);
        SpawnItem(duck, redTeamDuckSpawns[spawnIndex].position );
        SpawnItem(duck, blueCarouselDuckSpawn.position);
    }

    void SpawnWarehouseItems()
    {
        for (int i = 0; i < objectsToSpawnInWarehouse.Length; i++)
        {
            for (int j = 0; j < numObjectsToSpawn[i]; j++)
            {
                Vector3 offset = new Vector3(Random.Range(-itemOffset, itemOffset), 0, Random.Range(-itemOffset, itemOffset));
                SpawnItem(objectsToSpawnInWarehouse[i], blueItemSpawn.position + offset);
            }
        }

        for (int i = 0; i < objectsToSpawnInWarehouse.Length; i++)
        {
            for (int j = 0; j < numObjectsToSpawn[i]; j++)
            {
                Vector3 offset = new Vector3(Random.Range(-itemOffset, itemOffset), 0, Random.Range(-itemOffset, itemOffset));
                SpawnItem(objectsToSpawnInWarehouse[i], redItemSpawn.position + offset);
            }
        }
    }

    public void StartGame()
    {
        if (gameStarted) return;
        SpawnDucks();
        SpawnWarehouseItems();
        GameObject.FindGameObjectWithTag("JointHub").BroadcastMessage("Reset");
        GameObject.FindGameObjectWithTag("RedHub").BroadcastMessage("Reset");
        GameObject.FindGameObjectWithTag("BlueHub").BroadcastMessage("Reset");
        GameObject[] g = GameObject.FindGameObjectsWithTag("Box");
        foreach (GameObject G in g) { G.BroadcastMessage("Reset"); }

        timer.StartGame();
        scoreKeeper.resetScore();
        gameStarted = true;
    }

    public void EndGame()
    {
        gameStarted = false;
        foreach (GameObject obj in spawnedItems)
        {
            Destroy(obj);
        }
    }

    void SpawnItem(GameObject objToSpawn,Vector3 pos)
    {
        spawnedItems.Add(Instantiate(objToSpawn, pos, objToSpawn.transform.rotation));
    }
}
