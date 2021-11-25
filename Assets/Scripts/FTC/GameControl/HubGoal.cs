using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubGoal : MonoBehaviour
{
    public ScoreKeeper scoreKeeper;
    public GameTimer gameTimer;

    bool isBalanced=true;

    public Transform parent;

    string team = "";

    public int HubLevel;

    bool gamovr = true;

    private void Awake()
    {
        if (!scoreKeeper) {
            scoreKeeper = GameObject.Find("ScoreKeeper").GetComponent<ScoreKeeper>();
            
        }
        if (!gameTimer) {
            gameTimer = GameObject.Find("ScoreKeeper").GetComponent<GameTimer>();
        }

        if (parent.name.ToLower().Contains("red")) { team = "Red"; }
        if (parent.name.ToLower().Contains("blue")) { team = "Blue"; }
    }

    private void Update()
    {
        if (HubLevel < 4)
        {
            bool deltaBal = isBalanced;
            int balScore = 0;
            if (parent.localEulerAngles.x > scoreKeeper.BalanceThreshold) { balScore += 1; }
            if (parent.localEulerAngles.x < -scoreKeeper.BalanceThreshold) { balScore += 1; }
            if (parent.localEulerAngles.z > scoreKeeper.BalanceThreshold) { balScore += 1; }
            if (parent.localEulerAngles.z < -scoreKeeper.BalanceThreshold) { balScore += 1; }
            if (balScore > 0) { isBalanced = false; } else { isBalanced = true; }
            if (deltaBal != isBalanced)
            {
                deltaBal = isBalanced;
            }
        }

        if (gameTimer.getTimer() <= 0f && !gamovr) { GameOver(); }
    }

   // List<Collider> CountedBalancedColliders = new List<Collider>();
    List<Collider> Countedcolliders = new List<Collider>();
    bool active = false;
    private void OnTriggerStay(Collider collider)
    {
        //in game score
        if ((collider.tag == "Cube" || collider.tag == "Ball" || collider.tag == "Duck") && HubLevel <4 && !Countedcolliders.Contains(collider))
        {
            if (team == "Blue")
            {
                active = true;
                if (HubLevel == 1) { scoreKeeper.addScoreBlue(scoreKeeper.HubLevel1Score); }
                if (HubLevel == 2) { scoreKeeper.addScoreBlue(scoreKeeper.HubLevel2Score); }
                if (HubLevel == 3) { scoreKeeper.addScoreBlue(scoreKeeper.HubLevel3Score); }
            }
        
            if (team == "Red")
            {
                active = true;
                if (HubLevel == 1) { scoreKeeper.addScoreRed(scoreKeeper.HubLevel1Score); }
                if (HubLevel == 2) { scoreKeeper.addScoreRed(scoreKeeper.HubLevel2Score); }
                if (HubLevel == 3) { scoreKeeper.addScoreRed(scoreKeeper.HubLevel3Score); }
            }
           
            Countedcolliders.Add(collider);
        }

        //End of Game
        /*if (collider.tag != "Cube" && collider.tag != "Ball" && collider.tag != "Duck" || CountedBalancedColliders.Contains(collider)) { return; }
        if (gameTimer.getTimer() > 0) { return; }

        if (team == "Blue")
        {
            if (isBalanced)
            {
                if (HubLevel == 1) { scoreKeeper.addScoreBlue(scoreKeeper.BalancedHubScore); }
                if (HubLevel == 2) { scoreKeeper.addScoreBlue(scoreKeeper.BalancedHubScore); }
                if (HubLevel == 3) { scoreKeeper.addScoreBlue(scoreKeeper.BalancedHubScore); }
            }
            if (!isBalanced)
            {
                if (HubLevel == 1) { scoreKeeper.addScoreBlue(scoreKeeper.UnbalancedHubScore); }
                if (HubLevel == 2) { scoreKeeper.addScoreBlue(scoreKeeper.UnbalancedHubScore); }
                if (HubLevel == 3) { scoreKeeper.addScoreBlue(scoreKeeper.UnbalancedHubScore); }
            }
        }
        if (team == "Red")
        {
            if (isBalanced)
            {
                if (HubLevel == 1) { scoreKeeper.addScoreRed(scoreKeeper.BalancedHubScore); }
                if (HubLevel == 2) { scoreKeeper.addScoreRed(scoreKeeper.BalancedHubScore); }
                if (HubLevel == 3) { scoreKeeper.addScoreRed(scoreKeeper.BalancedHubScore); }
            }
            if (!isBalanced)
            {
                if (HubLevel == 1) { scoreKeeper.addScoreRed(scoreKeeper.UnbalancedHubScore); }
                if (HubLevel == 2) { scoreKeeper.addScoreRed(scoreKeeper.UnbalancedHubScore); }
                if (HubLevel == 3) { scoreKeeper.addScoreRed(scoreKeeper.UnbalancedHubScore); }
            }
        }
        CountedBalancedColliders.Add(collider);
        Destroy(collider.gameObject);*/
    }

    public void GameOver()
    {
        if (gamovr) { return; }
        
        if(isBalanced && HubLevel < 4 && !gamovr && active)
        {
            if (team == "Blue")
            {
                Debug.Log("HubGoal " + gameObject.name + " HUB#" + HubLevel + " " + team + " GameOver");
                scoreKeeper.addScoreBlue(scoreKeeper.BalancedHubScore);
            }
            if (team == "Red")
            {
                Debug.Log("HubGoal " + gameObject.name + " HUB#" + HubLevel + " " + team + " GameOver");
                scoreKeeper.addScoreRed(scoreKeeper.BalancedHubScore);
            }
        }
        if ( HubLevel == 4 && !gamovr && active)
        {
            if (transform.localEulerAngles.z < 0f)
            {
                Debug.Log("HubGoal " + gameObject.name + " HUB#" + HubLevel + " " + team + " GameOver");
                scoreKeeper.addScoreBlue(scoreKeeper.UnbalancedHubScore);
            }
            if (transform.localEulerAngles.z > 0f)
            {
                Debug.Log("HubGoal " + gameObject.name + " HUB#" + HubLevel + " " + team + " GameOver");
                scoreKeeper.addScoreRed(scoreKeeper.UnbalancedHubScore);
            }
        }
        gamovr = true;

    }

    public void Reset()
    {
        gamovr = false;
        active = false;
        Countedcolliders = new List<Collider>();
        //CountedBalancedColliders = new List<Collider>();
    }

}
