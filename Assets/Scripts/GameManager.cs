using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] APIManager apiManager;
    private string player;
    private string otherplayer;
    private bool disabled = false;
    private bool GameRunning = false;
    private bool waitingForEnd = false;
    private int questionsLeft = 0;
    private Dictionary<string, string> MatchStats;

    public void RunGame(Dictionary<string, string> newMatchStats)
    {
        MatchStats = newMatchStats;
        GameRunning = true;
        player = "P" + MatchStats["YouAre"];
        otherplayer = "P" + (1 + (int.Parse(MatchStats["YouAre"])%2));
        questionsLeft = int.Parse(MatchStats[player + "QuestionsLeft"]);
        LoadQuestion();
        while (GameRunning)
        {
            if (!disabled)
            {
                uiManager.UpdateScores(MatchStats[player + "Score"], MatchStats[otherplayer + "Score"], MatchStats[player + "QuestionsLeft"], MatchStats[otherplayer + "QuestionsLeft"]);
                questionsLeft = int.Parse(MatchStats[player + "QuestionsLeft"]);
                RefreshStats();
                StartCoroutine(Cooldown(1));
                if (questionsLeft <= 0) GameRunning = false;
            }
        }

        waitingForEnd = true;
        uiManager.LoadEndScreen();
        while (waitingForEnd)
        {
            if (!disabled)
            {
                uiManager.UpdateScores(MatchStats[player + "Score"], MatchStats[otherplayer + "Score"], MatchStats[player + "QuestionsLeft"], MatchStats[otherplayer + "QuestionsLeft"]);
                questionsLeft = int.Parse(MatchStats[otherplayer + "QuestionsLeft"]);
                RefreshStats();
                StartCoroutine(Cooldown(1));
                if (questionsLeft <= 0) waitingForEnd = false;
            }
        }

        string message = "";

        if (int.Parse(MatchStats[player+"Score"])> int.Parse(MatchStats[otherplayer + "Score"]))
        {
            message = "Congragulations!\n" + "you have won the match";   
        }
        else if (int.Parse(MatchStats[player + "Score"]) < int.Parse(MatchStats[otherplayer + "Score"]))
        {
            message = "Womp Womp...\n" + "it seems that your opponent has defeated you";
        }
        else
        {
            message = "WOAH!\n" + "it seems that this match was a tie!";
        }
        uiManager.updateEndMessage(message);


    }


    public bool isGameRunning(string player)
    {
        return GameRunning;
    }

    public void LoadQuestion()
    {
        if (GameRunning)
        {
            StartCoroutine(apiManager.GetNextQuestion());
        }
    }

    private void RefreshStats()
    {
        StartCoroutine(apiManager.GetMatchStatus((Status) => { MatchStats = Status; }));
    }

    private IEnumerator Cooldown(int cooldown)
    {
        disabled = true;
        yield return new WaitForSeconds(cooldown);
        disabled = false;
    }
}
