using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxGoal : MonoBehaviour
{
    public ScoreKeeper scoreKeeper;
    public GameTimer gameTimer;

    string team="";
    string type = "";

    bool gamovr = true;

    private void Awake()
    {
        if (!scoreKeeper)
        {
            scoreKeeper = GameObject.Find("ScoreKeeper").GetComponent<ScoreKeeper>();
        }
        if (!gameTimer)
        {
            gameTimer = GameObject.Find("ScoreKeeper").GetComponent<GameTimer>();
        }
        if (gameObject.name.ToLower().Contains("red")) { team = "Red"; }
        if (gameObject.name.ToLower().Contains("blue")) { team = "Blue"; }
        if (gameObject.name.ToLower().Contains("warehouse")) { type = "warehouse"; }
        if (gameObject.name.ToLower().Contains("storage")) { type = "storage"; }
    }

    private void Update()
    {
        if (gameTimer.getTimer() <= 0f && !gamovr) { GameOver(); }
    }

    int triggers = 0;
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "RobotTrigger") { triggers += 1; }
        Debug.Log(gameObject.name + " triggers: " + triggers);
    }
    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "RobotTrigger") { triggers -= 1; }
        Debug.Log(gameObject.name + " triggers: " + triggers);
    }

    void GameOver()
    {
        if (gamovr) { return; }
        if (triggers == 4)
        {
            if (team == "Blue")
            {
                if (type == "warehouse") { scoreKeeper.addScoreBlue(scoreKeeper.WarehouseCompleteScore); }
                if (type == "storage") { scoreKeeper.addScoreBlue(scoreKeeper.StorageCompleteScore); }
            }
          
            if (team == "Red") {
                if (type == "warehouse") { scoreKeeper.addScoreRed(scoreKeeper.WarehouseCompleteScore); }
                if (type == "storage") { scoreKeeper.addScoreRed(scoreKeeper.StorageCompleteScore); }
            }
            
        }
        else
        {
            if (triggers > 0)
            {
                if (team == "Blue")
                {
                    if (type == "warehouse") { scoreKeeper.addScoreBlue(scoreKeeper.WarehousePartialScore); }
                    if (type == "storage") { scoreKeeper.addScoreBlue(scoreKeeper.StoragePartialScore); }
                }
               
                if (team == "Red")
                {
                    if (type == "warehouse") { scoreKeeper.addScoreRed(scoreKeeper.WarehousePartialScore); }
                    if (type == "storage") { scoreKeeper.addScoreRed(scoreKeeper.StoragePartialScore); }
                }
            }
        }
        gamovr = true;
    }
    public void Reset()
    {
        triggers = 0;
        gamovr = false;
    }
}
