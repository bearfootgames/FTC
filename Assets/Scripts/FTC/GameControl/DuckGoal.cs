using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckGoal : MonoBehaviour
{
    public ScoreKeeper scoreKeeper;
    public GameTimer gameTimer;

    bool gamovr=true;

    string team;

    // Start is called before the first frame update
    void Awake()
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

    }

    void Update()
    {
        if (gameTimer.getTimer() <= 0 && !gamovr) { GameOver(); }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (gameTimer.getTimer() > 30) { return; }
        if (collider.tag != "duck") { return; }
        if (team == "Blue") { scoreKeeper.addScoreBlue(scoreKeeper.DuckScore); }
        if (team == "Red") { scoreKeeper.addScoreRed(scoreKeeper.DuckScore); }

    }
    private void OnTriggerStay(Collider collider)
    {
        if (gameTimer.getTimer() <= 0)
        {
            if (collider.tag == "Duck")
            {
                if (team == "Blue") { scoreKeeper.addScoreBlue(scoreKeeper.DuckScore); }
                if (team == "Red") { scoreKeeper.addScoreRed(scoreKeeper.DuckScore); }
                Destroy(collider.gameObject);
            }
        }
    }

    void GameOver()
    {
        gamovr = true;
    }
    public void Reset()
    {
        gamovr = false;
    }
}
